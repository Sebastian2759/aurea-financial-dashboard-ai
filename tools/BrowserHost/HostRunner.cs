using Test.Integration;

namespace DashboardBrowserHost;
internal static class HostRunner
{
    public static async Task Main()
    {
        var databaseFile = Path.Combine(AppContext.BaseDirectory, "aurea-local.db");
        using var factory = new DashboardFactory(databaseFile, useLiveMarketData: true);
        factory.UseKestrel(5080);
        using var client = factory.Client();
        Console.WriteLine("LOCAL_HOST_READY http://localhost:5080 (SQLite persistente / CoinGecko real / simulación deshabilitada)");
        var stop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Console.CancelKeyPress += (_, args) => { args.Cancel = true; stop.TrySetResult(); };
        await stop.Task;
    }
}
