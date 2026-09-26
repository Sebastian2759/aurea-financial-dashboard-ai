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
namespace Application.UseCases.Sessions.CreateDemoSession;
public sealed class CreateDemoSessionCommand(ISessionTokenIssuer issuer) : IRequestHandler<CreateDemoSessionRequest, ResponseBase<CreateDemoSessionResponse>>
{
    public async Task<ResponseBase<CreateDemoSessionResponse>> Handle(CreateDemoSessionRequest request, CancellationToken ct)
    {

        await Task.CompletedTask;
        if (!issuer.IsEnabled) throw new DemoDisabledException();
        var user = DemoUsers.Find(request.UserId)!;
        var ticket = issuer.Issue(user);
        return ResponseBase<CreateDemoSessionResponse>.Ok(new(ticket.AccessToken, ticket.ExpiresAt, user.Id, user.Name, user.Role, Permissions.ForRole(user.Role)));

    }
}
