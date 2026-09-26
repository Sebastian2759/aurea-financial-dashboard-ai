namespace Domain.Markets;
public static class VolatilityCalculator
{
    public static double? Calculate(IEnumerable<PricePoint> points, DateTimeOffset now)
    {
        var currentHour = now.ToUnixTimeSeconds() / 3600;
        var closes = points.Where(p => p.Time.ToUnixTimeSeconds() / 3600 < currentHour)
            .GroupBy(p => p.Time.ToUnixTimeSeconds() / 3600)
            .Select(g => (Hour: g.Key, Price: g.OrderBy(p => p.Time).Last().Price))
            .OrderBy(p => p.Hour).TakeLast(25).ToArray();
        if (closes.Length != 25 || closes.Any(p => !double.IsFinite(p.Price) || p.Price <= 0) || closes[^1].Hour != currentHour - 1 || closes.Where((p, i) => i > 0 && p.Hour - closes[i - 1].Hour != 1).Any()) return null;
        var returns = closes.Skip(1).Select((p, i) => Math.Log(p.Price / closes[i].Price)).ToArray();
        var average = returns.Average();
        return Math.Sqrt(returns.Sum(r => Math.Pow(r - average, 2)) / 23) * Math.Sqrt(24) * 100;
    }
}
