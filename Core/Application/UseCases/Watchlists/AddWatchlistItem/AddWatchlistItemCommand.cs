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
namespace Application.UseCases.Watchlists.AddWatchlistItem;
public sealed class AddWatchlistItemCommand(ICurrentUser user, IWatchlistItemRepository repository, IAuditEventRepository audit, IMapperAdapter mapper) : IRequestHandler<AddWatchlistItemRequest, ResponseBase<AddWatchlistItemResponse>>
{
    public async Task<ResponseBase<AddWatchlistItemResponse>> Handle(AddWatchlistItemRequest request, CancellationToken ct)
    {

        AccessRules.RequireOwner(user, request.OwnerId, Permissions.WatchlistWrite);
        if (await repository.ExistsAsync(x => x.OwnerId == request.OwnerId && x.CoinId == request.CoinId, ct))
            return ResponseBase<AddWatchlistItemResponse>.Conflict("El activo ya está en esta lista.");
        var entity = new WatchlistItemEntity { OwnerId = request.OwnerId, CoinId = request.CoinId, Note = request.Note?.Trim(), CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        repository.Add(entity);
        audit.Add(AuditRecord.Create(user, "watchlist.add", entity.Id.ToString(), entity.OwnerId, null, new { entity.CoinId, entity.Note }));
        await repository.CommitAsync(ct);
        return ResponseBase<AddWatchlistItemResponse>.Created(new(mapper.Map<WatchlistItemEntity, WatchlistItemDto>(entity)));

    }
}
