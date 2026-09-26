using Application.UseCases.Watchlists.GetWatchlist;
using Application.UseCases.Watchlists.GetWatchlistOwners;
using Application.UseCases.Watchlists.AddWatchlistItem;
using Application.UseCases.Watchlists.UpdateWatchlistItem;
using Application.UseCases.Watchlists.RemoveWatchlistItem;
using Asp.Versioning;
using Domain.Contracts.Security;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1;
public sealed record WatchlistItemInput(string CoinId, string? Note);
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/watchlists")]
public sealed class WatchlistsController(IMediator mediator, ICurrentUser user) : ApiControllerBase
{
    private string Owner(string ownerId) => ownerId == "me" ? user.Id : ownerId;
    [HttpGet("owners"), Authorize(Policy = Permissions.WatchlistManageAll)]
    public async Task<IActionResult> Owners(CancellationToken ct) => ToResult(await mediator.Send(new GetWatchlistOwnersRequest(), ct));
    [HttpGet("{ownerId}/items"), Authorize(Policy = Permissions.WatchlistRead)]
    public async Task<IActionResult> List(string ownerId, CancellationToken ct) => ToResult(await mediator.Send(new GetWatchlistRequest(Owner(ownerId)), ct));
    [HttpPost("{ownerId}/items"), Authorize(Policy = Permissions.WatchlistWrite)]
    public async Task<IActionResult> Add(string ownerId, WatchlistItemInput input, CancellationToken ct)
        => ToResult(await mediator.Send(new AddWatchlistItemRequest(Owner(ownerId), input.CoinId, input.Note), ct));
    [HttpPut("{ownerId}/items/{itemId:guid}"), Authorize(Policy = Permissions.WatchlistWrite)]
    public async Task<IActionResult> Update(string ownerId, Guid itemId, WatchlistItemInput input, CancellationToken ct)
        => ToResult(await mediator.Send(new UpdateWatchlistItemRequest(Owner(ownerId), itemId, input.CoinId, input.Note), ct));
    [HttpDelete("{ownerId}/items/{itemId:guid}"), Authorize(Policy = Permissions.WatchlistWrite)]
    public async Task<IActionResult> Remove(string ownerId, Guid itemId, CancellationToken ct)
        => ToResult(await mediator.Send(new RemoveWatchlistItemRequest(Owner(ownerId), itemId), ct));
}
