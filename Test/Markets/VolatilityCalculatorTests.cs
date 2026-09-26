using Domain.Markets;
namespace Test.Markets;
public sealed class VolatilityCalculatorTests
{
    private static readonly DateTimeOffset Now=new(2026,9,24,12,0,0,TimeSpan.Zero);
    [Fact]
    public void ConstantPricesHaveZeroVolatility()
    {
        var points=Enumerable.Range(0,25).Select(i=>new PricePoint(Now.AddHours(i-25),100));
        Assert.Equal(0,VolatilityCalculator.Calculate(points,Now));
    }
    [Fact]
    public void AlternatingReturnsMatchSampleStandardDeviation()
    {
        var points=new List<PricePoint>{new(Now.AddHours(-25),100)};
        for(var i=1;i<=24;i++) points.Add(new(Now.AddHours(i-25),points[^1].Price*Math.Exp(i%2==0?0.01:-0.01)));
        var expected=Math.Sqrt(24*0.0001/23)*Math.Sqrt(24)*100;
        Assert.Equal(expected,VolatilityCalculator.Calculate(points,Now)!.Value,9);
    }
    [Fact]
    public void MissingOrStaleWindowIsUnavailable()
    {
        var points=Enumerable.Range(0,25).Select(i=>new PricePoint(Now.AddHours(i-25),100)).ToArray();
        Assert.Null(VolatilityCalculator.Calculate(points.Skip(1),Now));
        Assert.Null(VolatilityCalculator.Calculate(points,Now.AddHours(2)));
    }
    [Fact]
    public void InvalidFinalCloseCannotReuseAnEarlierPriceFromTheSameHour()
    {
        var points=Enumerable.Range(0,25).Select(i=>new PricePoint(Now.AddHours(i-25),100)).Append(new PricePoint(Now.AddMinutes(-1),0));
        Assert.Null(VolatilityCalculator.Calculate(points,Now));
    }
}
