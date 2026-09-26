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
namespace Application.UseCases.Markets.GetAssetHistory;
public sealed class GetAssetHistoryQuery(IMarketDataProvider provider) : IRequestHandler<GetAssetHistoryRequest, ResponseBase<GetAssetHistoryResponse>>
{
    public async Task<ResponseBase<GetAssetHistoryResponse>> Handle(GetAssetHistoryRequest request, CancellationToken ct)
    {

        var history = await provider.GetHistoryAsync(request.CoinId, request.Currency, ct);
        var minimum = DateTimeOffset.UtcNow.AddDays(-request.Days);
        return ResponseBase<GetAssetHistoryResponse>.Ok(new(history with { Points = history.Points.Where(p => p.Time >= minimum).ToArray() }));

    }
}
