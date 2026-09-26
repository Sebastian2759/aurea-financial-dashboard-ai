using Domain.Base.Interface;
using Domain.Entities;
namespace Domain.Contracts.Persistence;
public sealed record SystemEventPage(IReadOnlyList<SystemEventEntity> Items, int Total);
public interface ISystemEventRepository : IRepositoryGeneric<SystemEventEntity>
{
    Task<SystemEventPage> ListAsync(int page, int pageSize, string? level, string? component, DateTime? from, DateTime? to, CancellationToken ct);
}
