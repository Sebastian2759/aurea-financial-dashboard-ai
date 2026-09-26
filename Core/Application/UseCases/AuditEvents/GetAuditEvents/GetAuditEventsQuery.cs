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
namespace Application.UseCases.AuditEvents.GetAuditEvents;
public sealed class GetAuditEventsQuery(ICurrentUser user, IAuditEventRepository repository, IMapperAdapter mapper) : IRequestHandler<GetAuditEventsRequest, ResponseBase<GetAuditEventsResponse>>
{
    public async Task<ResponseBase<GetAuditEventsResponse>> Handle(GetAuditEventsRequest request, CancellationToken ct)
    {

        AccessRules.Require(user, Permissions.AuditRead);
        var result = await repository.ListAsync(request.Page, request.PageSize, request.ActorId, request.Action, ct);
        return ResponseBase<GetAuditEventsResponse>.Ok(new(result.Items.Select(x => mapper.Map<AuditEventEntity, AuditEventDto>(x)).ToArray(), result.Total, request.Page, request.PageSize));

    }
}
