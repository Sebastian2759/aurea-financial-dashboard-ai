using Domain.Base;
namespace Domain.Entities;
public sealed class SystemEventEntity : EntityBase
{
    public string Code { get; set; } = "";
    public string Level { get; set; } = "";
    public string Component { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Currency { get; set; }
    public string? CoinId { get; set; }
}
