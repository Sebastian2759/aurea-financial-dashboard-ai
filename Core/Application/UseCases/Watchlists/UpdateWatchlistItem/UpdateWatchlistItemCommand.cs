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
namespace Application.UseCases.Watchlists.UpdateWatchlistItem;
public sealed class UpdateWatchlistItemCommand(ICurrentUser user, IWatchlistItemRepository repository, IAuditEventRepository audit, IMapperAdapter mapper) : IRequestHandler<UpdateWatchlistItemRequest, ResponseBase<UpdateWatchlistItemResponse>>
{
    public async Task<ResponseBase<UpdateWatchlistItemResponse>> Handle(UpdateWatchlistItemRequest request, CancellationToken ct)
    {

        AccessRules.RequireOwner(user, request.OwnerId, Permissions.WatchlistWrite);
        var entity = await repository.FindFirstOrDefaultAsync(x => x.Id == request.ItemId && x.OwnerId == request.OwnerId, ct);
        if (entity is null) return ResponseBase<UpdateWatchlistItemResponse>.NotFound();
        if (await repository.ExistsAsync(x => x.OwnerId == request.OwnerId && x.CoinId == request.CoinId && x.Id != request.ItemId, ct))
            return ResponseBase<UpdateWatchlistItemResponse>.Conflict("El activo ya está en esta lista.");
        var before = new { entity.CoinId, entity.Note };
        entity.CoinId = request.CoinId; entity.Note = request.Note?.Trim(); entity.UpdatedAtUtc = DateTime.UtcNow;
        repository.Edit(entity);
        audit.Add(AuditRecord.Create(user, "watchlist.update", entity.Id.ToString(), entity.OwnerId, before, new { entity.CoinId, entity.Note }));
        await repository.CommitAsync(ct);
        return ResponseBase<UpdateWatchlistItemResponse>.Ok(new(mapper.Map<WatchlistItemEntity, WatchlistItemDto>(entity)));

    }
}
