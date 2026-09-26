namespace Application.Dtos;
public sealed class WatchlistItemDto
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = "";
    public string CoinId { get; set; } = "";
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
