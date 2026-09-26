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
namespace Application.UseCases.DashboardSettings.GetThresholds;
public sealed class GetThresholdsQuery(IRepositoryGeneric<DashboardSettingsEntity> repository) : IRequestHandler<GetThresholdsRequest, ResponseBase<GetThresholdsResponse>>
{
    public async Task<ResponseBase<GetThresholdsResponse>> Handle(GetThresholdsRequest request, CancellationToken ct)
    {

        var settings = await repository.FindFirstOrDefaultAsync(x => x.SingletonKey == 1, ct);
        return ResponseBase<GetThresholdsResponse>.Ok(new(settings?.VolatilityThreshold ?? 5));

    }
}
