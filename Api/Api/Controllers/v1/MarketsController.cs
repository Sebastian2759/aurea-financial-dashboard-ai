using Application.UseCases.Markets.GetMarketSnapshot;
using Application.UseCases.Markets.GetAssetHistory;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers.v1;
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/markets")]
[Authorize(Policy = Permissions.MarketRead)]
public sealed class MarketsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string currency = "usd", CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetMarketSnapshotRequest(currency), ct));
    [HttpGet("{coinId}/history")]
    public async Task<IActionResult> History(string coinId, [FromQuery] string currency = "usd", [FromQuery] int days = 7, CancellationToken ct = default)
        => ToResult(await mediator.Send(new GetAssetHistoryRequest(coinId, currency, days), ct));
}
