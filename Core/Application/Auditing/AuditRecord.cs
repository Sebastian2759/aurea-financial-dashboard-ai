using System.Text.Json;
using Domain.Contracts.Security;
using Domain.Entities;
namespace Application.Auditing;
public static class AuditRecord
{
    public static AuditEventEntity Create(ICurrentUser user, string action, string resource, string? ownerId, object? before, object? after)
        => new() { ActorId = user.Id, Action = action, Resource = resource, OwnerId = ownerId,
            Before = before is null ? null : JsonSerializer.Serialize(before), After = after is null ? null : JsonSerializer.Serialize(after),
            CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
}
