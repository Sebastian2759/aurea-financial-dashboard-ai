using Domain.Diagnostics;
namespace Domain.Contracts.Diagnostics;

// Structured events deliberately exclude arbitrary messages, exceptions and request data.
public interface ISystemEventSink
{
    void Record(SystemEventKind kind, string? currency = null, string? coinId = null);
}
