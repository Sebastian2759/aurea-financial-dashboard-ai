using Application.UseCases.SystemEvents.GetSystemEvents;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/system-events")]
[Authorize(Policy = Permissions.SystemLogsRead)]
public sealed class SystemEventsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetSystemEventsRequest request, CancellationToken ct)
        => ToResult(await mediator.Send(request, ct));
}
