using Application.Base;
using Domain.Markets;
namespace Test.Architecture;
public sealed class DependencyTests
{
    [Fact]
    public void InnerLayersDoNotReferencePersistenceTransportOrAdapters()
    {
        var forbidden = new[] { "Persistence", "Api", "Adapters", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore.SignalR" };
        foreach (var assembly in new[] { typeof(MarketQuote).Assembly, typeof(ResponseBase<>).Assembly })
            Assert.DoesNotContain(assembly.GetReferencedAssemblies(), a => forbidden.Any(name => a.Name!.StartsWith(name, StringComparison.Ordinal)));
    }
}
