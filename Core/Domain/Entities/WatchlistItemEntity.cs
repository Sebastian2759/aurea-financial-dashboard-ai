using Domain.Base;
namespace Domain.Entities;
public sealed class WatchlistItemEntity : EntityBase
{
    public string OwnerId { get; set; } = "";
    public string CoinId { get; set; } = "";
    public string? Note { get; set; }
}
