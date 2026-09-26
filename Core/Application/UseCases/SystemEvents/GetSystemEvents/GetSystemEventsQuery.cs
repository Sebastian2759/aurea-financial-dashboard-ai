using Application.Base;
using Application.Dtos;
using Application.Security;
using Domain.Contracts.Adapter.Mapper;
using Domain.Contracts.Persistence;
using Domain.Contracts.Security;
using Domain.Entities;
using Domain.Security;
using MediatR;
namespace Application.UseCases.SystemEvents.GetSystemEvents;
public sealed class GetSystemEventsQuery(ICurrentUser user, ISystemEventRepository repository, IMapperAdapter mapper)
    : IRequestHandler<GetSystemEventsRequest, ResponseBase<GetSystemEventsResponse>>
{
    public async Task<ResponseBase<GetSystemEventsResponse>> Handle(GetSystemEventsRequest request, CancellationToken ct)
    {
        AccessRules.Require(user, Permissions.SystemLogsRead);
        var result = await repository.ListAsync(request.Page, request.PageSize, request.Level, request.Component, request.From?.UtcDateTime, request.To?.UtcDateTime, ct);
        return ResponseBase<GetSystemEventsResponse>.Ok(new(result.Items.Select(x => mapper.Map<SystemEventEntity, SystemEventDto>(x)).ToArray(), result.Total, request.Page, request.PageSize));
    }
}
