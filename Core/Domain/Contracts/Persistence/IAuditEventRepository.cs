using Domain.Base.Interface;
using Domain.Entities;
namespace Domain.Contracts.Persistence;
public sealed record AuditPage(IReadOnlyList<AuditEventEntity> Items, int Total);
public interface IAuditEventRepository : IRepositoryGeneric<AuditEventEntity>
{
    Task<AuditPage> ListAsync(int page, int pageSize, string? actorId, string? action, CancellationToken ct);
}
