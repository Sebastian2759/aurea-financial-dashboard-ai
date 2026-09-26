using Domain.Contracts.Persistence;
using Domain.Diagnostics;
namespace Api.Diagnostics;
public sealed class SystemEventWriter(SystemEventBuffer buffer, IServiceScopeFactory scopes, ILogger<SystemEventWriter> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        buffer.Record(SystemEventKind.ApplicationStarted);
        try
        {
            await foreach (var item in buffer.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ISystemEventRepository>();
                    repository.Add(item);
                    await repository.CommitAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception)
                {
                    // Logging failures cannot fail a market request or recurse into this queue.
                    logger.LogWarning("No se pudo persistir un evento técnico.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
