using Domain.Markets;
namespace Domain.Contracts.Adapter.MarketData;
public interface IMarketDataProvider
{
    Task<MarketSnapshot> GetSnapshotAsync(string currency, CancellationToken ct);
    Task<AssetHistory> GetHistoryAsync(string coinId, string currency, CancellationToken ct);
}
