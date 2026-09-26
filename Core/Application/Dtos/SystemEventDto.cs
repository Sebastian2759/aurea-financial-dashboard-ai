namespace Application.Dtos;
public sealed class SystemEventDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Level { get; set; } = "";
    public string Component { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Currency { get; set; }
    public string? CoinId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
