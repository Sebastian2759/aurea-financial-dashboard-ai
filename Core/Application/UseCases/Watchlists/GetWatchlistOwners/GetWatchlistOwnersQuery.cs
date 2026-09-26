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
namespace Application.UseCases.Watchlists.GetWatchlistOwners;
public sealed class GetWatchlistOwnersQuery(ICurrentUser user) : IRequestHandler<GetWatchlistOwnersRequest, ResponseBase<GetWatchlistOwnersResponse>>
{
    public async Task<ResponseBase<GetWatchlistOwnersResponse>> Handle(GetWatchlistOwnersRequest request, CancellationToken ct)
    {

        await Task.CompletedTask;
        AccessRules.Require(user, Permissions.WatchlistManageAll);
        return ResponseBase<GetWatchlistOwnersResponse>.Ok(new(DemoUsers.All.Where(x => x.Role is "Trader" or "Admin").ToArray()));

    }
}
