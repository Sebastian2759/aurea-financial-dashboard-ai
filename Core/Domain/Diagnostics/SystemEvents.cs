namespace Domain.Diagnostics;

public enum SystemEventKind
{
    ApplicationStarted, ProviderRateLimited, ProviderFailed, DemoFallbackUsed,
    StaleDataServed, BroadcastFailed, ThresholdNotificationFailed, RequestFailed
}

public sealed record SystemEventDescription(string Code, string Level, string Component, string Message);

public static class SystemEventCatalog
{
    public static SystemEventDescription Describe(SystemEventKind kind) => kind switch
    {
        SystemEventKind.ApplicationStarted => new("application.started", "Information", "Api", "La aplicación está disponible."),
        SystemEventKind.ProviderRateLimited => new("market.rate-limited", "Warning", "Market", "CoinGecko limitó las solicitudes; se respeta el período de espera."),
        SystemEventKind.ProviderFailed => new("market.provider-failed", "Warning", "Market", "No se pudo obtener una respuesta válida de CoinGecko."),
        SystemEventKind.DemoFallbackUsed => new("market.demo-fallback", "Warning", "Market", "Se utilizaron datos simulados identificados para mantener disponible la demostración."),
        SystemEventKind.StaleDataServed => new("market.stale-data", "Warning", "Market", "Se conservaron los últimos datos reales disponibles y se marcaron desactualizados."),
        SystemEventKind.BroadcastFailed => new("market.broadcast-failed", "Error", "Market", "No se pudo completar la actualización periódica del mercado."),
        SystemEventKind.ThresholdNotificationFailed => new("thresholds.notification-failed", "Warning", "Api", "El umbral se guardó, pero no se pudo enviar la notificación."),
        SystemEventKind.RequestFailed => new("request.failed", "Error", "Api", "Una operación terminó con un error interno."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
