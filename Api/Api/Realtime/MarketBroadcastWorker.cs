using Application.UseCases.Markets.GetMarketSnapshot;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
namespace Api.Realtime;
public sealed class MarketBroadcastWorker(IServiceScopeFactory scopes, MarketSubscriptions subscriptions, IHubContext<MarketHub> hub, ILogger<MarketBroadcastWorker> logger, IConfiguration configuration, ISystemEventSink events) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(configuration.GetValue("Market:RefreshSeconds", 60)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var currency in subscriptions.ActiveCurrencies)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new GetMarketSnapshotRequest(currency), stoppingToken);
                    if (result.Data is not null) await hub.Clients.Group("market:" + currency).SendAsync("MarketSnapshotUpdated", result.Data.Snapshot, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception ex)
                {
                    events.Record(SystemEventKind.BroadcastFailed, currency);
                    logger.LogWarning(ex, "No se pudo actualizar mercado {Currency}", currency);
                }
            }
        }
    }
}
