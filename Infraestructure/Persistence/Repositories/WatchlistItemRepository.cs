using Domain.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Base;
using Persistence.Context;
namespace Persistence.Repositories;
public sealed class WatchlistItemRepository(AppDbContext context) : RepositoryGeneric<WatchlistItemEntity>(context), IWatchlistItemRepository
{
    public async Task<IReadOnlyList<WatchlistItemEntity>> ListAsync(string ownerId, CancellationToken ct)
        => await DbSet.AsNoTracking().Where(x => x.OwnerId == ownerId).OrderBy(x => x.CreatedAtUtc).ToListAsync(ct);
}
