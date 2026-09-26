namespace Domain.Security;

public sealed record DemoUser(string Id, string Name, string Role);

public static class DemoUsers
{
    public static readonly IReadOnlyList<DemoUser> All = [
        new("viewer", "Lector", "Viewer"), new("trader-a", "Operador A", "Trader"),
        new("trader-b", "Operador B", "Trader"), new("admin", "Administrador", "Admin")];
    public static DemoUser? Find(string id) => All.FirstOrDefault(x => x.Id == id);
}

public static class Permissions
{
    public const string MarketRead = "market.read";
    public const string WatchlistRead = "watchlist.read";
    public const string WatchlistWrite = "watchlist.write";
    public const string WatchlistManageAll = "watchlist.manage-all";
    public const string ThresholdsRead = "thresholds.read";
    public const string ThresholdsWrite = "thresholds.write";
    public const string AuditRead = "audit.read";
    public const string SystemLogsRead = "system-logs.read";
    public static readonly string[] All = [MarketRead, WatchlistRead, WatchlistWrite, WatchlistManageAll, ThresholdsRead, ThresholdsWrite, AuditRead, SystemLogsRead];
    public static IReadOnlyList<string> ForRole(string role) => role switch
    {
        "Viewer" => [MarketRead, ThresholdsRead],
        "Trader" => [MarketRead, ThresholdsRead, WatchlistRead, WatchlistWrite],
        "Admin" => All,
        _ => []
    };
}

public sealed class AccessDeniedException() : Exception("No tienes permiso para realizar esta operación.");
public sealed class DemoDisabledException() : Exception("La autenticación de demostración no está habilitada.");
public sealed class MarketUnavailableException() : Exception("El proveedor de mercado no está disponible.");
