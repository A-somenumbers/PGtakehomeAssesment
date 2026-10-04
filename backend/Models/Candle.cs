namespace backend.Models
{
    public sealed record Candle(
        DateTimeOffset Time,
        decimal High,
        decimal Low,
        long Volume);

    public sealed record ChartData(
        string Symbol,
        string ExchangeTimeZoneID,
        IReadOnlyList<Candle> Candles);
}