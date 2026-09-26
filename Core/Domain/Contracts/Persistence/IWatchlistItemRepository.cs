using Domain.Base.Interface;
using Domain.Entities;
namespace Domain.Contracts.Persistence;
public interface IWatchlistItemRepository : IRepositoryGeneric<WatchlistItemEntity>
{
    Task<IReadOnlyList<WatchlistItemEntity>> ListAsync(string ownerId, CancellationToken ct);
}
