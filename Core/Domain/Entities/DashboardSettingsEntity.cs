using Domain.Base;
namespace Domain.Entities;
public sealed class DashboardSettingsEntity : EntityBase
{
    public double VolatilityThreshold { get; set; } = 5;
    public int SingletonKey { get; set; } = 1;
}
