using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
namespace Test.Integration;
public sealed class RbacHttpTests
{
    public static async Task<string> Login(HttpClient client, string user)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/demo-sessions", new { userId = user });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("data").GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return token;
    }
    [Theory]
    [InlineData("viewer", "/api/v1/markets", 200)]
    [InlineData("viewer", "/api/v1/watchlists/me/items", 403)]
    [InlineData("viewer", "/api/v1/audit-events", 403)]
    [InlineData("trader-a", "/api/v1/watchlists/me/items", 200)]
    [InlineData("trader-a", "/api/v1/watchlists/trader-b/items", 403)]
    [InlineData("trader-a", "/api/v1/watchlists/owners", 403)]
    [InlineData("trader-a", "/api/v1/audit-events", 403)]
    [InlineData("admin", "/api/v1/watchlists/trader-a/items", 200)]
    [InlineData("admin", "/api/v1/watchlists/owners", 200)]
    [InlineData("admin", "/api/v1/audit-events", 200)]
    public async Task RolesAreEnforcedByHttpPipeline(string user, string route, int expected)
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        await Login(client, user);
        Assert.Equal(expected, (int)(await client.GetAsync(route)).StatusCode);
    }
    [Fact]
    public async Task MissingInvalidExpiredAndApiKeyDoNotAuthenticateAsUser()
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/markets")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-only-api-key-00000000");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/markets")).StatusCode);
        var token = await Login(client,"admin");
        client.DefaultRequestHeaders.Authorization = new("Bearer", token[..^10] + "tampered00");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/markets")).StatusCode);
        var expired = new JwtSecurityToken("financial-dashboard","financial-dashboard-ui",[new Claim("sub","admin"),new Claim("permission","market.read")],DateTime.UtcNow.AddHours(-2),DateTime.UtcNow.AddHours(-1),new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DashboardFactory.SigningKey)),SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new("Bearer",new JwtSecurityTokenHandler().WriteToken(expired));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/markets")).StatusCode);
    }
    [Theory]
    [InlineData("viewer")]
    [InlineData("trader-a")]
    public async Task OnlyAdminChangesThresholds(string user)
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        await Login(client,user);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PutAsJsonAsync("/api/v1/dashboard/thresholds",new { volatilityThreshold=7 })).StatusCode);
    }
    [Fact]
    public async Task WatchlistCrudIsIsolatedAndAdminChangesAreAudited()
    {
        using var factory = new DashboardFactory(); using var a = factory.Client(); using var b = factory.Client(); using var admin = factory.Client();
        await Login(a,"trader-a"); await Login(b,"trader-b"); await Login(admin,"admin");
        var added = await a.PostAsJsonAsync("/api/v1/watchlists/me/items",new { coinId="bitcoin", note="Inicial", ownerId="trader-b" });
        Assert.Equal(HttpStatusCode.Created,added.StatusCode);
        var item=(await added.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("item");
        var id=item.GetProperty("id").GetString(); Assert.Equal("trader-a",item.GetProperty("ownerId").GetString());
        Assert.Equal(HttpStatusCode.Conflict,(await a.PostAsJsonAsync("/api/v1/watchlists/me/items",new { coinId="bitcoin" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await b.PutAsJsonAsync($"/api/v1/watchlists/trader-a/items/{id}",new { coinId="ethereum" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await b.DeleteAsync($"/api/v1/watchlists/me/items/{id}")).StatusCode);
        var changed = await admin.PutAsJsonAsync($"/api/v1/watchlists/trader-a/items/{id}",new { coinId="ethereum",note="Cambio administrativo" });
        Assert.Equal(HttpStatusCode.OK,changed.StatusCode);
        using var freshClient=factory.Client(); await Login(freshClient,"trader-a");
        var persisted=(await freshClient.GetFromJsonAsync<JsonElement>("/api/v1/watchlists/me/items")).GetProperty("data").GetProperty("items");
        Assert.Equal("ethereum",persisted[0].GetProperty("coinId").GetString());
        Assert.Equal(HttpStatusCode.OK,(await admin.DeleteAsync($"/api/v1/watchlists/trader-a/items/{id}")).StatusCode);
        var audit=(await admin.GetFromJsonAsync<JsonElement>("/api/v1/audit-events?actorId=admin")).GetProperty("data");
        Assert.Equal(2,audit.GetProperty("total").GetInt32());
        Assert.All(audit.GetProperty("items").EnumerateArray(),e => { Assert.Equal("admin",e.GetProperty("actorId").GetString()); Assert.Equal("trader-a",e.GetProperty("ownerId").GetString()); });
    }
    [Fact]
    public async Task InvalidInputsDoNotMutateAndThresholdPersists()
    {
        using var factory = new DashboardFactory(); using var client = factory.Client();
        await Login(client,"admin");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PutAsJsonAsync("/api/v1/dashboard/thresholds",new { volatilityThreshold=-1 })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync("/api/v1/dashboard/thresholds",new { volatilityThreshold=7.25 })).StatusCode);
        Assert.Equal(7.25,(await client.GetFromJsonAsync<JsonElement>("/api/v1/dashboard/thresholds")).GetProperty("data").GetProperty("volatilityThreshold").GetDouble());
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/v1/watchlists/me/items",new { coinId="invalid" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.GetAsync("/api/v1/markets?currency=invalid")).StatusCode);
    }
}
