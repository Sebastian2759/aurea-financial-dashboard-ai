using Domain.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Base;
using Persistence.Context;
namespace Persistence.Repositories;
public sealed class SystemEventRepository(AppDbContext context) : RepositoryGeneric<SystemEventEntity>(context), ISystemEventRepository
{
    public async Task<SystemEventPage> ListAsync(int page, int pageSize, string? level, string? component, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var query = DbSet.AsNoTracking();
        if (!string.IsNullOrEmpty(level)) query = query.Where(x => x.Level == level);
        if (!string.IsNullOrEmpty(component)) query = query.Where(x => x.Component == component);
        if (from.HasValue) query = query.Where(x => x.CreatedAtUtc >= from);
        if (to.HasValue) query = query.Where(x => x.CreatedAtUtc <= to);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(items, total);
    }
}
