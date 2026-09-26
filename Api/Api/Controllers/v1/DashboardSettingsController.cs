using Api.Realtime;
using Application.UseCases.DashboardSettings.GetThresholds;
using Application.UseCases.DashboardSettings.UpdateThresholds;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;
namespace Api.Controllers.v1;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard/thresholds")]
public sealed class DashboardSettingsController(IMediator mediator, IHubContext<MarketHub> hub, ILogger<DashboardSettingsController> logger, ISystemEventSink events) : ApiControllerBase
{
    [HttpGet, Authorize(Policy = Permissions.ThresholdsRead)]
    public async Task<IActionResult> Get(CancellationToken ct) => ToResult(await mediator.Send(new GetThresholdsRequest(), ct));
    [HttpPut, Authorize(Policy = Permissions.ThresholdsWrite)]
    public async Task<IActionResult> Update(UpdateThresholdsRequest request, CancellationToken ct)
    {
        var response = await mediator.Send(request, ct);
        if (response.IsSuccess)
        {
            try { await hub.Clients.All.SendAsync("ThresholdsUpdated", response.Data, ct); }
            catch (Exception ex)
            {
                events.Record(SystemEventKind.ThresholdNotificationFailed);
                logger.LogWarning(ex, "Umbral persistido; la notificación se recuperará al reconectar o recargar.");
            }
        }
        return ToResult(response);
    }
}
