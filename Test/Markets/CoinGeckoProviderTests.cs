using System.Net;
using System.Text;
using System.Text.Json;
using Adapters.MarketData;
using Domain.Markets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
using Moq;
namespace Test.Markets;
public sealed class CoinGeckoProviderTests
{
    private sealed class Clock : TimeProvider { public DateTimeOffset Now=DateTimeOffset.UtcNow; public override DateTimeOffset GetUtcNow()=>Now; }
    private sealed class Handler : HttpMessageHandler
    {
        public int Requests; public HttpStatusCode Status=HttpStatusCode.OK; public bool NullPrice; public bool Timeout; public bool InvalidHistory; public int HistoryDelayHours; public DateTimeOffset UpdatedAt=DateTimeOffset.UtcNow;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Interlocked.Increment(ref Requests);
            if(Timeout) throw new TaskCanceledException("Provider timeout");
            if(Status!=HttpStatusCode.OK) return Task.FromResult(new HttpResponseMessage(Status));
            object payload=request.RequestUri!.AbsolutePath.EndsWith("markets")
                ? MarketCatalog.Assets.Select(a=>new { id=a.Id, current_price=NullPrice?(decimal?)null:100,price_change_percentage_24h=(double?)1,market_cap=(decimal?)null,total_volume=(decimal?)null,last_updated=UpdatedAt.ToString("O") }).ToArray()
                : new { prices=Enumerable.Range(0,169).Select(i=>new double[]{DateTimeOffset.UtcNow.AddHours(i-168-HistoryDelayHours).ToUnixTimeMilliseconds(),InvalidHistory && i==167?0:100+i}).ToArray() };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(payload),Encoding.UTF8,"application/json")});
        }
    }
    private sealed class Factory(Handler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name)=>new(handler,false){BaseAddress=new Uri("https://coingecko.test/")}; }
    private static CoinGeckoMarketDataProvider Provider(Handler handler,Clock clock,bool fallback=true,ISystemEventSink? events=null)=>new(new Factory(handler),Options.Create(new MarketDataOptions{AllowDemoFallback=fallback,RefreshSeconds=60,HistoryCacheSeconds=300}),clock,NullLogger<CoinGeckoMarketDataProvider>.Instance,events??Mock.Of<ISystemEventSink>());
    [Fact]
    public async Task NullMetricsRemainNullWithoutSwitchingToSimulation()
    {
        var provider=Provider(new Handler{NullPrice=true},new Clock());
        var result=await provider.GetSnapshotAsync("usd",default);
        Assert.Equal("coingecko",result.Source);
        Assert.All(result.Quotes,q=> { Assert.Null(q.Price); Assert.Null(q.MarketCap); Assert.Null(q.Volume24h); });
    }
    [Fact]
    public async Task ConcurrentReadersShareCacheAndFailurePreservesLastRealTimestamp()
    {
        var clock=new Clock(); var handler=new Handler(); var provider=Provider(handler,clock);
        var results=await Task.WhenAll(Enumerable.Range(0,8).Select(_=>provider.GetSnapshotAsync("usd",default)));
        Assert.Equal(4,handler.Requests);
        Assert.All(results,r=>Assert.Equal("coingecko",r.Source));
        var original=results[0].FetchedAt; clock.Now=clock.Now.AddSeconds(61); handler.Status=HttpStatusCode.TooManyRequests;
        var stale=await provider.GetSnapshotAsync("usd",default);
        Assert.True(stale.IsStale); Assert.Equal(original,stale.FetchedAt); Assert.Equal("coingecko",stale.Source);
    }
    [Fact]
    public async Task RateLimitHasExplicitSimulationAndNoRetryStorm()
    {
        var handler=new Handler{Status=HttpStatusCode.TooManyRequests}; var provider=Provider(handler,new Clock());
        Assert.Equal("demo",(await provider.GetSnapshotAsync("usd",default)).Source);
        Assert.Equal("demo",(await provider.GetSnapshotAsync("eur",default)).Source);
        Assert.Equal(1,handler.Requests);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TemporaryFailuresRetryAtMostThreeTimesAndPreserveRealData(bool timeout)
    {
        var handler=new Handler();var clock=new Clock();var provider=Provider(handler,clock);
        var original=await provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken);
        clock.Now=clock.Now.AddSeconds(61);handler.Timeout=timeout;handler.Status=HttpStatusCode.ServiceUnavailable;
        var stale=await provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken);
        Assert.True(stale.IsStale);Assert.Equal(original.FetchedAt,stale.FetchedAt);Assert.Equal(7,handler.Requests);
    }
    [Fact]
    public async Task OldSourceTimestampIsNeverAdvertisedAsFresh()
    {
        var provider=Provider(new Handler{UpdatedAt=DateTimeOffset.UtcNow.AddMinutes(-10)},new Clock());
        var result=await provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken);
        Assert.Equal("coingecko",result.Source);Assert.True(result.IsStale);
    }
    [Fact]
    public async Task InvalidHistoryDoesNotProduceAnInventedClosingPrice()
    {
        var provider=Provider(new Handler{InvalidHistory=true},new Clock());
        var history=await provider.GetHistoryAsync("bitcoin","usd",TestContext.Current.CancellationToken);
        Assert.Equal("demo",history.Source);
        var snapshot=await provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken);
        Assert.Equal("coingecko",snapshot.Source);
        Assert.All(snapshot.Quotes,q=>Assert.Null(q.Volatility24h));
    }
    [Fact]
    public async Task OldHistoryIsMarkedStaleAndCannotCalculateCurrentVolatility()
    {
        var provider=Provider(new Handler{HistoryDelayHours=3},new Clock());
        var history=await provider.GetHistoryAsync("bitcoin","usd",TestContext.Current.CancellationToken);
        Assert.Equal("coingecko",history.Source);Assert.True(history.IsStale);
        var snapshot=await provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken);
        Assert.All(snapshot.Quotes,q=>Assert.Null(q.Volatility24h));
    }
    [Fact]
    public async Task SimulationIsNotUsedWhenDisabled()
    {
        var provider=Provider(new Handler{Status=HttpStatusCode.TooManyRequests},new Clock(),false);
        await Assert.ThrowsAsync<Domain.Security.MarketUnavailableException>(()=>provider.GetSnapshotAsync("usd",TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<Domain.Security.MarketUnavailableException>(()=>provider.GetHistoryAsync("bitcoin","usd",TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RateLimitAndFallbackProduceStructuredSystemEvents()
    {
        var events = new Mock<ISystemEventSink>();
        await Provider(new Handler { Status = HttpStatusCode.TooManyRequests }, new Clock(), events: events.Object)
            .GetSnapshotAsync("usd", TestContext.Current.CancellationToken);
        events.Verify(x => x.Record(SystemEventKind.ProviderRateLimited, null, null), Times.Once);
        events.Verify(x => x.Record(SystemEventKind.ProviderFailed, "usd", null), Times.Once);
        events.Verify(x => x.Record(SystemEventKind.DemoFallbackUsed, "usd", null), Times.Once);
    }

    [Fact]
    public async Task FailedRefreshRecordsStaleRealDataInsteadOfSimulation()
    {
        var events = new Mock<ISystemEventSink>();
        var handler = new Handler(); var clock = new Clock();
        var provider = Provider(handler, clock, events: events.Object);
        await provider.GetSnapshotAsync("usd", TestContext.Current.CancellationToken);
        clock.Now = clock.Now.AddMinutes(2); handler.Status = HttpStatusCode.ServiceUnavailable;
        var snapshot = await provider.GetSnapshotAsync("usd", TestContext.Current.CancellationToken);
        Assert.Equal("coingecko", snapshot.Source); Assert.True(snapshot.IsStale);
        events.Verify(x => x.Record(SystemEventKind.StaleDataServed, "usd", null), Times.Once);
        events.Verify(x => x.Record(SystemEventKind.DemoFallbackUsed, It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }
}
