using System.Net.Http.Json;
using System.Text.Json;
namespace Test.Integration;
public sealed class PersistenceRestartTests
{
    [Fact]
    public async Task WatchlistAndAuditSurviveHostRestart()
    {
        var directory=Path.Combine(Path.GetTempPath(),"aurea-tests");Directory.CreateDirectory(directory);
        var file=Path.Combine(directory,Guid.NewGuid()+".db");
        try
        {
            using(var first=new DashboardFactory(file))
            {
                using var trader=first.Client();await RbacHttpTests.Login(trader,"trader-a");
                var added=await trader.PostAsJsonAsync("/api/v1/watchlists/me/items",new{coinId="solana",note="Conservar tras reinicio"},TestContext.Current.CancellationToken);
                Assert.Equal(System.Net.HttpStatusCode.Created,added.StatusCode);
            }
            using var second=new DashboardFactory(file);using var client=second.Client();await RbacHttpTests.Login(client,"trader-a");
            using var items=JsonDocument.Parse(await client.GetStringAsync("/api/v1/watchlists/me/items",TestContext.Current.CancellationToken));
            var item=Assert.Single(items.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
            Assert.Equal("solana",item.GetProperty("coinId").GetString());Assert.EndsWith("Z",item.GetProperty("createdAtUtc").GetString());
            await RbacHttpTests.Login(client,"admin");
            using var audit=JsonDocument.Parse(await client.GetStringAsync("/api/v1/audit-events",TestContext.Current.CancellationToken));
            Assert.Equal(1,audit.RootElement.GetProperty("data").GetProperty("total").GetInt32());
        }
        finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();File.Delete(file); }
    }
}
