using System.Collections.Concurrent;
namespace Api.Realtime;
public sealed class MarketSubscriptions
{
    private readonly ConcurrentDictionary<string, string> subscriptions = new();
    public string? Set(string connectionId, string currency)
    {
        subscriptions.TryGetValue(connectionId, out var previous);
        subscriptions[connectionId] = currency;
        return previous;
    }
    public void Remove(string connectionId) => subscriptions.TryRemove(connectionId, out _);
    public IReadOnlyList<string> ActiveCurrencies => subscriptions.Values.Distinct().ToArray();
}
