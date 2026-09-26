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
namespace Application.UseCases.DashboardSettings.UpdateThresholds;
public sealed class UpdateThresholdsCommand(ICurrentUser user, IRepositoryGeneric<DashboardSettingsEntity> repository, IAuditEventRepository audit) : IRequestHandler<UpdateThresholdsRequest, ResponseBase<UpdateThresholdsResponse>>
{
    public async Task<ResponseBase<UpdateThresholdsResponse>> Handle(UpdateThresholdsRequest request, CancellationToken ct)
    {

        AccessRules.Require(user, Permissions.ThresholdsWrite);
        var settings = await repository.FindFirstOrDefaultAsync(x => x.SingletonKey == 1, ct);
        var before = settings?.VolatilityThreshold ?? 5;
        if (settings is null) { settings = new DashboardSettingsEntity { CreatedAtUtc = DateTime.UtcNow }; repository.Add(settings); }
        settings.VolatilityThreshold = request.VolatilityThreshold; settings.UpdatedAtUtc = DateTime.UtcNow;
        audit.Add(AuditRecord.Create(user, "thresholds.update", "global", null, new { VolatilityThreshold = before }, new { settings.VolatilityThreshold }));
        await repository.CommitAsync(ct);
        return ResponseBase<UpdateThresholdsResponse>.Ok(new(settings.VolatilityThreshold));

    }
}
