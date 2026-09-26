using Domain.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Base;
using Persistence.Context;
namespace Persistence.Repositories;
public sealed class AuditEventRepository(AppDbContext context) : RepositoryGeneric<AuditEventEntity>(context), IAuditEventRepository
{
    public async Task<AuditPage> ListAsync(int page, int pageSize, string? actorId, string? action, CancellationToken ct)
    {
        var query = DbSet.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(actorId)) query = query.Where(x => x.ActorId == actorId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(items, total);
    }
}
