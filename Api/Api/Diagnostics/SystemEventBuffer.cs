using System.Threading.Channels;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
using Domain.Entities;
using Domain.Markets;
namespace Api.Diagnostics;
public sealed class SystemEventBuffer(TimeProvider clock) : ISystemEventSink
{
    private readonly Channel<SystemEventEntity> queue = Channel.CreateBounded<SystemEventEntity>(
        new BoundedChannelOptions(256) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
    public ChannelReader<SystemEventEntity> Reader => queue.Reader;

    public void Record(SystemEventKind kind, string? currency = null, string? coinId = null)
    {
        var description = SystemEventCatalog.Describe(kind);
        var now = clock.GetUtcNow().UtcDateTime;
        queue.Writer.TryWrite(new SystemEventEntity
        {
            Code = description.Code, Level = description.Level, Component = description.Component,
            Message = description.Message, CreatedAtUtc = now, UpdatedAtUtc = now,
            Currency = currency is not null && MarketCatalog.HasCurrency(currency) ? currency : null,
            CoinId = coinId is not null && MarketCatalog.HasAsset(coinId) ? coinId : null
        });
    }
}
