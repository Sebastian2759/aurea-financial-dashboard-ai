using Application.UseCases.AuditEvents.GetAuditEvents;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-events")]
[Authorize(Policy = Permissions.AuditRead)]
public sealed class AuditEventsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetAuditEventsRequest request, CancellationToken ct) => ToResult(await mediator.Send(request, ct));
}
