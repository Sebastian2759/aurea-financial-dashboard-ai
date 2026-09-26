using Application.UseCases.Markets.GetMarketSnapshot;
using Domain.Markets;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace Api.Realtime;
[Authorize(Policy = Permissions.MarketRead)]
public sealed class MarketHub(IMediator mediator, MarketSubscriptions subscriptions) : Hub
{
    public async Task Subscribe(string currency)
    {
        if (!MarketCatalog.HasCurrency(currency)) throw new HubException("Moneda no soportada.");
        var previous = subscriptions.Set(Context.ConnectionId, currency);
        if (previous is not null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, "market:" + previous);
        await Groups.AddToGroupAsync(Context.ConnectionId, "market:" + currency);
        var result = await mediator.Send(new GetMarketSnapshotRequest(currency), Context.ConnectionAborted);
        if (result.Data is not null) await Clients.Caller.SendAsync("MarketSnapshotUpdated", result.Data.Snapshot, Context.ConnectionAborted);
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        subscriptions.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
