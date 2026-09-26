using Application.UseCases.Sessions.CreateDemoSession;
using Application.UseCases.Sessions.GetCurrentSession;
using Asp.Versioning;
using Domain.Contracts.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class SessionsController(IMediator mediator, ISessionTokenIssuer issuer) : ApiControllerBase
{
    [AllowAnonymous, HttpPost("demo-sessions")]
    public async Task<IActionResult> Create(CreateDemoSessionRequest request, CancellationToken ct)
        => !issuer.IsEnabled ? NotFound() : ToResult(await mediator.Send(request, ct));
    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct) => ToResult(await mediator.Send(new GetCurrentSessionRequest(), ct));
}
