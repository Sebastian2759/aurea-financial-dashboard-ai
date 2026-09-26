using Domain.Markets;
namespace Adapters.MarketData;
public static class DemoMarketData
{
    public static AssetHistory History(string coinId, string currency, DateTimeOffset now)
    {
        var basis = coinId switch { "bitcoin" => 68000d, "ethereum" => 3400d, _ => 145d };
        if (currency == "eur") basis *= 0.92;
        var hour = DateTimeOffset.FromUnixTimeSeconds(now.ToUnixTimeSeconds() / 3600 * 3600);
        var points = Enumerable.Range(0, 169).Select(i => new PricePoint(hour.AddHours(i - 168), basis * (1 + 0.018 * Math.Sin((hour.ToUnixTimeSeconds() / 3600 - 168 + i) / 5d) + 0.007 * Math.Cos(i / 2d)))).ToArray();
        return new(coinId, currency, points, now, "demo", false);
    }
    public static MarketSnapshot Snapshot(string currency, DateTimeOffset now)
    {
        var quotes = MarketCatalog.Assets.Select(asset =>
        {
            var history = History(asset.Id, currency, now);
            var price = history.Points[^1].Price;
            var previous = history.Points[^25].Price;
            var supply = asset.Id switch { "bitcoin" => 19_800_000d, "ethereum" => 120_000_000d, _ => 450_000_000d };
            return new MarketQuote(asset.Id, asset.Symbol, asset.Name, (decimal)price, (price / previous - 1) * 100, (decimal)(price * supply), (decimal)(price * supply * 0.035), VolatilityCalculator.Calculate(history.Points, now), now);
        }).ToArray();
        return new(currency, quotes, now, "demo", false, "Datos simulados de demostración. Configura CoinGecko para obtener cotizaciones reales.");
    }
}
