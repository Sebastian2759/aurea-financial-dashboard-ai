using Domain.Base;
namespace Domain.Entities;
public sealed class AuditEventEntity : EntityBase
{
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Resource { get; set; } = "";
    public string? OwnerId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
}
