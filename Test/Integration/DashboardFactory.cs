using Adapters.MarketData;
using Domain.Contracts.Adapter.MarketData;
using Domain.Markets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Persistence.Context;
namespace Test.Integration;
public sealed class DashboardFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "test-only-signing-key-00000000000000000000000000000000";
    private readonly SqliteConnection connection;
    private readonly bool useLiveMarketData;
    private readonly string signingKey;
    public DashboardFactory(string? databaseFile = null, bool useLiveMarketData = false)
    {
        this.useLiveMarketData = useLiveMarketData;
        signingKey = useLiveMarketData ? Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) : SigningKey;
        connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = databaseFile ?? ":memory:" }.ToString());
        connection.Open();
        if (useLiveMarketData)
        {
            using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
            db.Database.EnsureCreated();
        }
    }
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = signingKey, ["ApiKey:Key"] = "test-only-api-key-00000000"
        }));
        return base.CreateHost(builder);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["Jwt:Secret"] = signingKey, ["Jwt:Issuer"] = "financial-dashboard", ["Jwt:Audience"] = "financial-dashboard-ui",
            ["ApiKey:Key"] = "test-only-api-key-00000000", ["DemoAuth:Enabled"] = "true", ["Database:Initialize"] = "false",
            ["Market:RefreshSeconds"] = useLiveMarketData ? "60" : "1", ["Market:AllowDemoFallback"] = "false"
        }));
        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(connection));
            if (!useLiveMarketData)
            {
                services.RemoveAll<IMarketDataProvider>();
                services.AddSingleton<IMarketDataProvider, TestMarketProvider>();
            }
        });
    }
    public HttpClient Client()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        return client;
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
    private sealed class TestMarketProvider : IMarketDataProvider
    {
        public Task<MarketSnapshot> GetSnapshotAsync(string currency, CancellationToken ct) => Task.FromResult(DemoMarketData.Snapshot(currency, DateTimeOffset.UtcNow));
        public Task<AssetHistory> GetHistoryAsync(string coinId, string currency, CancellationToken ct) => Task.FromResult(DemoMarketData.History(coinId, currency, DateTimeOffset.UtcNow));
    }
}
