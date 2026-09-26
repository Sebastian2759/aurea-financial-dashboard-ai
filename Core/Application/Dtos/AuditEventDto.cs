namespace Application.Dtos;
public sealed class AuditEventDto
{
    public Guid Id { get; set; }
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Resource { get; set; } = "";
    public string? OwnerId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
