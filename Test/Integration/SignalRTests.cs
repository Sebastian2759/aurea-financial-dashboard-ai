using Domain.Markets;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
namespace Test.Integration;
public sealed class SignalRTests
{
    [Fact]
    public async Task TwoAuthenticatedClientsReceiveSnapshotsAndCanResubscribe()
    {
        using var factory=new DashboardFactory(); using var client=factory.Client();
        var token=await RbacHttpTests.Login(client,"viewer");
        HubConnection Connect() => new HubConnectionBuilder().WithUrl(new Uri(client.BaseAddress!,"/hubs/market"),o => { o.AccessTokenProvider=()=>Task.FromResult<string?>(token); o.HttpMessageHandlerFactory=_=>factory.Server.CreateHandler(); o.Transports=HttpTransportType.LongPolling; }).Build();
        await using var first=Connect(); await using var second=Connect();
        var a=new TaskCompletionSource<MarketSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var b=new TaskCompletionSource<MarketSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        first.On<MarketSnapshot>("MarketSnapshotUpdated",s=>a.TrySetResult(s));
        second.On<MarketSnapshot>("MarketSnapshotUpdated",s=>b.TrySetResult(s));
        await first.StartAsync(); await second.StartAsync();
        await first.InvokeAsync("Subscribe","usd"); await second.InvokeAsync("Subscribe","usd");
        Assert.Equal("usd",(await a.Task.WaitAsync(TimeSpan.FromSeconds(10))).Currency);
        Assert.Equal("usd",(await b.Task.WaitAsync(TimeSpan.FromSeconds(10))).Currency);
        // A periodic broadcast is produced once and delivered to both subscribers.
        a=new(TaskCreationOptions.RunContinuationsAsynchronously);
        b=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var broadcastA=await a.Task.WaitAsync(TimeSpan.FromSeconds(10),TestContext.Current.CancellationToken);
        var broadcastB=await b.Task.WaitAsync(TimeSpan.FromSeconds(10),TestContext.Current.CancellationToken);
        Assert.Equal(broadcastA.FetchedAt,broadcastB.FetchedAt);
        await first.StopAsync(); await first.StartAsync();
        var eur=new TaskCompletionSource<MarketSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        first.On<MarketSnapshot>("MarketSnapshotUpdated",s=> { if(s.Currency=="eur") eur.TrySetResult(s); });
        await first.InvokeAsync("Subscribe","eur");
        Assert.Equal("eur",(await eur.Task.WaitAsync(TimeSpan.FromSeconds(10))).Currency);
    }
}
