namespace Adapters.MarketData;
public sealed class MarketDataOptions
{
    public string BaseUrl { get; set; } = "https://api.coingecko.com/api/v3/";
    public string ApiKey { get; set; } = "";
    public int RefreshSeconds { get; set; } = 60;
    public int HistoryCacheSeconds { get; set; } = 300;
    public bool AllowDemoFallback { get; set; }
}
