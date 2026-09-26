using Api.Controllers.v1;
using Api.Diagnostics;
using Application.Base;
using Application.Dtos;
using Application.UseCases.SystemEvents.GetSystemEvents;
using Domain.Contracts.Adapter.Mapper;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Test.Integration;
namespace Test.SystemEvents;

public sealed class SystemEventsTests
{
    [Theory]
    [InlineData("viewer", 403)]
    [InlineData("trader-a", 403)]
    [InlineData("trader-b", 403)]
    [InlineData("admin", 200)]
    public async Task SystemLogsAreAdminOnly(string user, int status)
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        await RbacHttpTests.Login(client, user);
        Assert.Equal(status, (int)(await client.GetAsync("/api/v1/system-events", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?level=Debug")]
    [InlineData("?component=DatabasePassword")]
    [InlineData("?from=2026-09-25T12:00:00Z&to=2026-09-24T12:00:00Z")]
    public async Task InvalidFiltersReturn400(string query)
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        await RbacHttpTests.Login(client, "admin");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/system-events" + query, TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task RealQueuePersistsFiltersAndPaginatesSafeEventsAcrossRestart()
    {
        var file = Path.Combine(Path.GetTempPath(), "aurea-system-events-" + Guid.NewGuid() + ".db");
        try
        {
            using (var factory = new DashboardFactory(file))
            {
                using var client = factory.Client(); await RbacHttpTests.Login(client, "admin");
                var sink = factory.Services.GetRequiredService<ISystemEventSink>();
                sink.Record(SystemEventKind.ProviderRateLimited, "usd", "bitcoin");
                sink.Record(SystemEventKind.DemoFallbackUsed, "SECRET_DO_NOT_STORE", "token=PRIVATE");
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                JsonElement data;
                do
                {
                    data = (await client.GetFromJsonAsync<JsonElement>("/api/v1/system-events?component=Market&level=Warning&pageSize=1", timeout.Token)).GetProperty("data");
                    if (data.GetProperty("total").GetInt32() == 2) break;
                    await Task.Delay(50, timeout.Token);
                } while (true);
                Assert.Equal(1, data.GetProperty("items").GetArrayLength());
                var second = (await client.GetFromJsonAsync<JsonElement>("/api/v1/system-events?component=Market&pageSize=1&page=2", timeout.Token)).GetProperty("data");
                Assert.NotEqual(data.GetProperty("items")[0].GetProperty("id").GetString(), second.GetProperty("items")[0].GetProperty("id").GetString());
                var empty = (await client.GetFromJsonAsync<JsonElement>("/api/v1/system-events?component=Market&level=Error", timeout.Token)).GetProperty("data");
                Assert.Equal(0, empty.GetProperty("total").GetInt32());
                var past = (await client.GetFromJsonAsync<JsonElement>("/api/v1/system-events?to=2000-01-01T00:00:00Z", timeout.Token)).GetProperty("data");
                Assert.Equal(0, past.GetProperty("total").GetInt32());
            }
            using var restarted = new DashboardFactory(file); using var fresh = restarted.Client();
            await RbacHttpTests.Login(fresh, "admin");
            var result = await fresh.GetStringAsync("/api/v1/system-events?component=Market", TestContext.Current.CancellationToken);
            Assert.DoesNotContain("SECRET_DO_NOT_STORE", result);
            Assert.DoesNotContain("PRIVATE", result);
            var items = JsonDocument.Parse(result).RootElement.GetProperty("data").GetProperty("items");
            Assert.Equal(2, items.GetArrayLength());
            Assert.All(items.EnumerateArray(), item => Assert.EndsWith("Z", item.GetProperty("createdAtUtc").GetString()));
            var fallback = items.EnumerateArray().Single(x => x.GetProperty("code").GetString() == "market.demo-fallback");
            Assert.Equal(JsonValueKind.Null, fallback.GetProperty("currency").ValueKind);
            Assert.Equal(JsonValueKind.Null, fallback.GetProperty("coinId").ValueKind);
        }
        finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); File.Delete(file); }
    }

    [Fact]
    public void CatalogRejectsUnknownEventsAndBufferIsBounded()
    {
        var buffer = new SystemEventBuffer(TimeProvider.System);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Record((SystemEventKind)999));
        for (var i = 0; i < 300; i++) buffer.Record(SystemEventKind.RequestFailed);
        var count = 0; while (buffer.Reader.TryRead(out _)) count++;
        Assert.Equal(256, count);
    }

    [Fact]
    public void ValidatorAcceptsSupportedFiltersAndDateRange()
    {
        var validator = new GetSystemEventsValidator();
        Assert.True(validator.Validate(new GetSystemEventsRequest(1, 20, "Warning", "Market", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow)).IsValid);
    }

    [Fact]
    public void EntityMapsAllPublicFieldsByConvention()
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        using var scope = factory.Services.CreateScope();
        var entity = new SystemEventEntity { Code = "market.rate-limited", Level = "Warning", Component = "Market", Message = "Mensaje", Currency = "eur", CoinId = "bitcoin", CreatedAtUtc = DateTime.UtcNow };
        var dto = scope.ServiceProvider.GetRequiredService<IMapperAdapter>().Map<SystemEventEntity, SystemEventDto>(entity);
        Assert.Equal(entity.Id, dto.Id); Assert.Equal(entity.Code, dto.Code); Assert.Equal(entity.Level, dto.Level);
        Assert.Equal(entity.Component, dto.Component); Assert.Equal(entity.Message, dto.Message);
        Assert.Equal(entity.Currency, dto.Currency); Assert.Equal(entity.CoinId, dto.CoinId); Assert.Equal(entity.CreatedAtUtc, dto.CreatedAtUtc);
    }

    [Fact]
    public async Task ControllerDelegatesExactFiltersToMediator()
    {
        var request = new GetSystemEventsRequest(2, 10, "Error", "Market");
        var expected = ResponseBase<GetSystemEventsResponse>.Ok(new([], 0, 2, 10));
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(request, TestContext.Current.CancellationToken)).ReturnsAsync(expected);
        var result = Assert.IsType<ObjectResult>(await new SystemEventsController(mediator.Object).Get(request, TestContext.Current.CancellationToken));
        Assert.Equal(200, result.StatusCode); Assert.Same(expected, result.Value);
        mediator.Verify(m => m.Send(request, TestContext.Current.CancellationToken), Times.Once);
    }
}
