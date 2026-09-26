using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace Test.Integration;
public sealed class PermissionMatrixTests
{
    private const string ItemId = "00000000-0000-0000-0000-000000000001";
    public static IEnumerable<object[]> ProtectedRoutes()
    {
        var routes = new (string Method, string Path)[]
        {
            ("GET", "/api/v1/auth/me"), ("GET", "/api/v1/markets"),
            ("GET", "/api/v1/markets/bitcoin/history"),
            ("GET", "/api/v1/watchlists/owners"), ("GET", "/api/v1/watchlists/me/items"),
            ("POST", "/api/v1/watchlists/me/items"), ("PUT", $"/api/v1/watchlists/me/items/{ItemId}"),
            ("DELETE", $"/api/v1/watchlists/me/items/{ItemId}"),
            ("GET", "/api/v1/dashboard/thresholds"), ("PUT", "/api/v1/dashboard/thresholds"),
            ("GET", "/api/v1/audit-events"), ("GET", "/api/v1/system-events"),
            ("POST", "/hubs/market/negotiate?negotiateVersion=1")
        };
        foreach (var route in routes)
            foreach (var apiKey in new[] { false, true })
                yield return [route.Method, route.Path, apiKey];
    }
    [Theory, MemberData(nameof(ProtectedRoutes))]
    public async Task EveryFunctionalRouteRejectsMissingJwtAndApiKey(string method, string path, bool apiKey)
    {
        using var factory = new DashboardFactory();
        using var client = factory.Client();
        if (apiKey) client.DefaultRequestHeaders.Add("X-Api-Key", "test-only-api-key-00000000");
        using var request = new HttpRequestMessage(new HttpMethod(method), path)
        { Content = JsonContent.Create(new { coinId = "bitcoin", volatilityThreshold = 5 }) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request, TestContext.Current.CancellationToken)).StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task ViewerCannotMutateWatchlists(string method)
    {
        using var factory = new DashboardFactory();
        using var client = factory.Client();
        await RbacHttpTests.Login(client, "viewer");
        using var request = new HttpRequestMessage(new HttpMethod(method),
            "/api/v1/watchlists/me/items" + (method == "POST" ? "" : "/" + ItemId))
        { Content = JsonContent.Create(new { coinId = "bitcoin", note = "No permitido" }) };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request, TestContext.Current.CancellationToken)).StatusCode);
        await RbacHttpTests.Login(client, "admin");
        var audit = await client.GetFromJsonAsync<JsonElement>("/api/v1/audit-events", TestContext.Current.CancellationToken);
        Assert.Equal(0, audit.GetProperty("data").GetProperty("total").GetInt32());
    }

    [Theory]
    [InlineData("trader-a")]
    [InlineData("trader-b")]
    public async Task TraderCompletesOwnCrudAndCannotTouchAnotherOwner(string trader)
    {
        using var factory = new DashboardFactory();
        using var client = factory.Client();
        await RbacHttpTests.Login(client, trader);
        var other = trader == "trader-a" ? "trader-b" : "trader-a";
        var added = await client.PostAsJsonAsync("/api/v1/watchlists/me/items", new { coinId = "bitcoin", note = "Inicial" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var id = (await added.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken))
            .GetProperty("data").GetProperty("item").GetProperty("id").GetGuid();
        var payload = new { coinId = "ethereum", note = "Edición propia" };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync($"/api/v1/watchlists/{other}/items", payload, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/v1/watchlists/{other}/items/{id}", payload, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/v1/watchlists/{other}/items/{id}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/v1/watchlists/me/items/{id}", payload, TestContext.Current.CancellationToken)).StatusCode);
        var list = (await client.GetFromJsonAsync<JsonElement>("/api/v1/watchlists/me/items", TestContext.Current.CancellationToken)).GetProperty("data").GetProperty("items");
        Assert.Equal("ethereum", list[0].GetProperty("coinId").GetString());
        Assert.Equal("Edición propia", list[0].GetProperty("note").GetString());
        await RbacHttpTests.Login(client, other);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/v1/watchlists/me/items/{id}", payload, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/v1/watchlists/me/items/{id}", TestContext.Current.CancellationToken)).StatusCode);
        await RbacHttpTests.Login(client, trader);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/v1/watchlists/me/items/{id}", TestContext.Current.CancellationToken)).StatusCode);
        await RbacHttpTests.Login(client, "admin");
        var audit = (await client.GetFromJsonAsync<JsonElement>($"/api/v1/audit-events?actorId={trader}", TestContext.Current.CancellationToken)).GetProperty("data");
        Assert.Equal(3, audit.GetProperty("total").GetInt32());
        Assert.All(audit.GetProperty("items").EnumerateArray(), item => Assert.Equal(trader, item.GetProperty("ownerId").GetString()));
    }
}
