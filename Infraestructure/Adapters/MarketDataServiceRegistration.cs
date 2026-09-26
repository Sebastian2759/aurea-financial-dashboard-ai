using Adapters.MarketData;
using Domain.Contracts.Adapter.MarketData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Adapters;
public static class MarketDataServiceRegistration
{
    public static IServiceCollection AddMarketDataServices(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        services.Configure<MarketDataOptions>(configuration.GetSection("Market"));
        services.PostConfigure<MarketDataOptions>(o => o.AllowDemoFallback &= environment is "Development" or "Demo" or "Testing");
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient("CoinGecko", client =>
        {
            client.BaseAddress = new Uri(configuration["Market:BaseUrl"] ?? "https://api.coingecko.com/api/v3/");
            client.Timeout = TimeSpan.FromSeconds(8);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FinancialDashboard/1.0");
        });
        services.AddSingleton<IMarketDataProvider, CoinGeckoMarketDataProvider>();
        return services;
    }
}
