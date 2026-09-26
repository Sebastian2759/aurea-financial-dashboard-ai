namespace Domain.Markets;
public sealed record Asset(string Id, string Symbol, string Name);
public static class MarketCatalog
{
    public static readonly IReadOnlyList<Asset> Assets = [new("bitcoin", "BTC", "Bitcoin"), new("ethereum", "ETH", "Ethereum"), new("solana", "SOL", "Solana")];
    public static bool HasAsset(string id) => Assets.Any(x => x.Id == id);
    public static bool HasCurrency(string currency) => currency is "usd" or "eur";
}
public sealed record PricePoint(DateTimeOffset Time, double Price);
public sealed record AssetHistory(string CoinId, string Currency, IReadOnlyList<PricePoint> Points, DateTimeOffset SourceUpdatedAt, string Source, bool IsStale);
public sealed record MarketQuote(string CoinId, string Symbol, string Name, decimal? Price, double? Change24h, decimal? MarketCap, decimal? Volume24h, double? Volatility24h, DateTimeOffset? UpdatedAt);
public sealed record MarketSnapshot(string Currency, IReadOnlyList<MarketQuote> Quotes, DateTimeOffset FetchedAt, string Source, bool IsStale, string? Message);
