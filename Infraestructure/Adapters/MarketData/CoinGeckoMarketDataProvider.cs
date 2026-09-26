using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Domain.Contracts.Adapter.MarketData;
using Domain.Markets;
using Domain.Security;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Adapters.MarketData;

public sealed class CoinGeckoMarketDataProvider(IHttpClientFactory clients, IOptions<MarketDataOptions> options, TimeProvider clock, ILogger<CoinGeckoMarketDataProvider> logger, ISystemEventSink events) : IMarketDataProvider
{
    private sealed record CacheEntry<T>(T Value, DateTimeOffset ExpiresAt);
    private readonly ConcurrentDictionary<string, CacheEntry<MarketSnapshot>> snapshots = new();
    private readonly ConcurrentDictionary<string, CacheEntry<AssetHistory>> histories = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> gates = new();
    private long retryAfterTicks;

    public async Task<MarketSnapshot> GetSnapshotAsync(string currency, CancellationToken ct)
    {
        if (!MarketCatalog.HasCurrency(currency)) throw new ArgumentException("Moneda no soportada.");
        var gate = gates.GetOrAdd("snapshot:" + currency, _ => new(1));
        await gate.WaitAsync(ct);
        try
        {
            if (snapshots.TryGetValue(currency, out var cached) && cached.ExpiresAt > clock.GetUtcNow()) return cached.Value;
            MarketSnapshot value;
            try
            {
                using var json = await FetchAsync($"coins/markets?vs_currency={currency}&ids=bitcoin,ethereum,solana&price_change_percentage=24h", ct);
                var quotes = new List<MarketQuote>();
                foreach (var asset in MarketCatalog.Assets)
                {
                    var raw = json.RootElement.EnumerateArray().FirstOrDefault(x => x.GetProperty("id").GetString() == asset.Id);
                    if (raw.ValueKind == JsonValueKind.Undefined) continue;
                    AssetHistory? history = null;
                    try { history = await GetHistoryAsync(asset.Id, currency, ct); }
                    catch (MarketUnavailableException) { /* Prices remain useful without a complete history. */ }
                    var volatility = history is { Source: "coingecko", IsStale: false } ? VolatilityCalculator.Calculate(history.Points, clock.GetUtcNow()) : null;
                    var updated = raw.TryGetProperty("last_updated", out var timestamp) && timestamp.ValueKind == JsonValueKind.String && timestamp.TryGetDateTimeOffset(out var date) ? date : (DateTimeOffset?)null;
                    quotes.Add(new(asset.Id, asset.Symbol, asset.Name, Decimal(raw,"current_price"), Double(raw,"price_change_percentage_24h"), Decimal(raw,"market_cap"), Decimal(raw,"total_volume"), volatility, updated));
                }
                if (quotes.Count == 0) throw new MarketUnavailableException();
                var stale = quotes.Any(q => q.UpdatedAt is null || q.UpdatedAt < clock.GetUtcNow().AddMinutes(-3));
                value = new(currency, quotes, clock.GetUtcNow(), "coingecko", stale,
                    stale ? "CoinGecko devolvió cotizaciones antiguas o sin fecha de origen verificable." : null);
            }
            catch (Exception ex) when (IsProviderFailure(ex, ct))
            {
                logger.LogWarning("Proveedor de mercado no disponible ({Failure}); se aplica respaldo explícito.", ex.GetType().Name);
                events.Record(SystemEventKind.ProviderFailed, currency);
                if (cached is not null && cached.Value.Source == "coingecko")
                {
                    value = cached.Value with { IsStale = true, Message = "Última cotización válida; proveedor temporalmente no disponible." };
                    events.Record(SystemEventKind.StaleDataServed, currency);
                }
                else if (options.Value.AllowDemoFallback)
                {
                    value = DemoMarketData.Snapshot(currency, clock.GetUtcNow());
                    events.Record(SystemEventKind.DemoFallbackUsed, currency);
                }
                else throw new MarketUnavailableException();
            }
            snapshots[currency] = new(value, clock.GetUtcNow().AddSeconds(Math.Max(1, options.Value.RefreshSeconds)));
            return value;
        }
        finally { gate.Release(); }
    }

    public async Task<AssetHistory> GetHistoryAsync(string coinId, string currency, CancellationToken ct)
    {
        if (!MarketCatalog.HasAsset(coinId) || !MarketCatalog.HasCurrency(currency)) throw new ArgumentException("Activo o moneda no soportados.");
        var key = coinId + ":" + currency;
        var gate = gates.GetOrAdd("history:" + key, _ => new(1));
        await gate.WaitAsync(ct);
        try
        {
            if (histories.TryGetValue(key, out var cached) && cached.ExpiresAt > clock.GetUtcNow()) return cached.Value;
            AssetHistory value;
            try
            {
                using var json = await FetchAsync($"coins/{coinId}/market_chart?vs_currency={currency}&days=7", ct);
                var prices = json.RootElement.GetProperty("prices").EnumerateArray().ToArray();
                // Reject an incomplete series rather than silently using an earlier price
                // as the closing price of an hour whose final sample was invalid.
                if (prices.Any(x => x.ValueKind != JsonValueKind.Array || x.GetArrayLength() < 2
                    || x[0].ValueKind != JsonValueKind.Number || !x[0].TryGetInt64(out _)
                    || x[1].ValueKind != JsonValueKind.Number || !x[1].TryGetDouble(out var price)
                    || !double.IsFinite(price) || price <= 0)) throw new MarketUnavailableException();
                var points = prices
                    .Select(x => new PricePoint(DateTimeOffset.FromUnixTimeMilliseconds(x[0].GetInt64()), x[1].GetDouble()))
                    .OrderBy(x => x.Time).ToArray();
                if (points.Length == 0) throw new MarketUnavailableException();
                value = new(coinId, currency, points, points[^1].Time, "coingecko", points[^1].Time < clock.GetUtcNow().AddHours(-2));
            }
            catch (Exception ex) when (IsProviderFailure(ex, ct))
            {
                events.Record(SystemEventKind.ProviderFailed, currency, coinId);
                if (cached is not null && cached.Value.Source == "coingecko")
                {
                    value = cached.Value with { IsStale = true };
                    events.Record(SystemEventKind.StaleDataServed, currency, coinId);
                }
                else if (options.Value.AllowDemoFallback)
                {
                    value = DemoMarketData.History(coinId, currency, clock.GetUtcNow());
                    events.Record(SystemEventKind.DemoFallbackUsed, currency, coinId);
                }
                else throw new MarketUnavailableException();
            }
            histories[key] = new(value, clock.GetUtcNow().AddSeconds(Math.Max(1, options.Value.HistoryCacheSeconds)));
            return value;
        }
        finally { gate.Release(); }
    }

    private async Task<JsonDocument> FetchAsync(string path, CancellationToken ct)
    {
        if (clock.GetUtcNow().UtcTicks < Interlocked.Read(ref retryAfterTicks)) throw new MarketUnavailableException();
        using var client = clients.CreateClient("CoinGecko");
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, path);
                if (!string.IsNullOrWhiteSpace(options.Value.ApiKey)) request.Headers.Add("x-cg-demo-api-key", options.Value.ApiKey);
                // Include downloading the body in HttpClient.Timeout's deadline.
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    events.Record(SystemEventKind.ProviderRateLimited);
                    var delay = response.Headers.RetryAfter?.Delta ?? (response.Headers.RetryAfter?.Date - clock.GetUtcNow()) ?? TimeSpan.FromSeconds(60);
                    Interlocked.Exchange(ref retryAfterTicks, clock.GetUtcNow().Add(delay > TimeSpan.Zero ? delay : TimeSpan.FromSeconds(60)).UtcTicks);
                    throw new MarketUnavailableException();
                }
                if ((int)response.StatusCode >= 500 && attempt < 2)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt) + Random.Shared.Next(100)), ct);
                    continue;
                }
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            }
            catch (Exception ex) when ((ex is HttpRequestException { StatusCode: null } or TaskCanceledException) && !ct.IsCancellationRequested && attempt < 2)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)), ct);
            }
        }
    }
    private static bool IsProviderFailure(Exception ex, CancellationToken ct) => !ct.IsCancellationRequested && ex is HttpRequestException or JsonException or InvalidOperationException or MarketUnavailableException or TaskCanceledException or KeyNotFoundException;
    private static decimal? Decimal(JsonElement value, string key) => value.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var n) ? n : null;
    private static double? Double(JsonElement value, string key) => value.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var n) && double.IsFinite(n) ? n : null;
}
