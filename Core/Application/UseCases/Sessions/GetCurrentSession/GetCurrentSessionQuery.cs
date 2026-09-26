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
namespace Application.UseCases.Sessions.GetCurrentSession;
public sealed class GetCurrentSessionQuery(ICurrentUser user) : IRequestHandler<GetCurrentSessionRequest, ResponseBase<GetCurrentSessionResponse>>
{
    public async Task<ResponseBase<GetCurrentSessionResponse>> Handle(GetCurrentSessionRequest request, CancellationToken ct)
    {

        await Task.CompletedTask;
        AccessRules.Require(user, Permissions.MarketRead);
        return ResponseBase<GetCurrentSessionResponse>.Ok(new(user.Id, DemoUsers.Find(user.Id)?.Name ?? user.Id, user.Role, Permissions.ForRole(user.Role)));

    }
}
