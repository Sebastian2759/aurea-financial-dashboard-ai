using Application.Base;
using Application.Dtos;
using Application.Security;
using Application.Auditing;
using Domain.Base.Interface;
using Domain.Entities;
using Domain.Markets;
using Domain.Security;
using Domain.Contracts.Security;
using Domain.Contracts.Persistence;
using Domain.Contracts.Adapter.Mapper;
using Domain.Contracts.Adapter.MarketData;
using FluentValidation;
using MediatR;
namespace Application.UseCases.Watchlists.RemoveWatchlistItem;
public sealed class RemoveWatchlistItemCommand(ICurrentUser user, IWatchlistItemRepository repository, IAuditEventRepository audit) : IRequestHandler<RemoveWatchlistItemRequest, ResponseBase<RemoveWatchlistItemResponse>>
{
    public async Task<ResponseBase<RemoveWatchlistItemResponse>> Handle(RemoveWatchlistItemRequest request, CancellationToken ct)
    {

        AccessRules.RequireOwner(user, request.OwnerId, Permissions.WatchlistWrite);
        var entity = await repository.FindFirstOrDefaultAsync(x => x.Id == request.ItemId && x.OwnerId == request.OwnerId, ct);
        if (entity is null) return ResponseBase<RemoveWatchlistItemResponse>.NotFound();
        repository.Delete(entity);
        audit.Add(AuditRecord.Create(user, "watchlist.remove", entity.Id.ToString(), entity.OwnerId, new { entity.CoinId, entity.Note }, null));
        await repository.CommitAsync(ct);
        return ResponseBase<RemoveWatchlistItemResponse>.Ok(new(true));

    }
}
