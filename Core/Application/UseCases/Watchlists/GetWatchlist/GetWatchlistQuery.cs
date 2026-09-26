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
namespace Application.UseCases.Watchlists.GetWatchlist;
public sealed class GetWatchlistQuery(ICurrentUser user, IWatchlistItemRepository repository, IMapperAdapter mapper) : IRequestHandler<GetWatchlistRequest, ResponseBase<GetWatchlistResponse>>
{
    public async Task<ResponseBase<GetWatchlistResponse>> Handle(GetWatchlistRequest request, CancellationToken ct)
    {

        AccessRules.RequireOwner(user, request.OwnerId, Permissions.WatchlistRead);
        var items = await repository.ListAsync(request.OwnerId, ct);
        return ResponseBase<GetWatchlistResponse>.Ok(new(items.Select(x => mapper.Map<WatchlistItemEntity, WatchlistItemDto>(x)).ToArray()));

    }
}
