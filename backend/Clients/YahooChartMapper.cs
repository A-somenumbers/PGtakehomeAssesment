using backend.Exceptions;
using backend.Models;
using backend.Models.Yahoo;

namespace backend.Clients;

internal static class YahooChartMapper
{
    public static ChartData Map(string RequestSymbol, YahooChartResponse response)
    {
        var chart = response.Chart
            ?? throw new UpstreamServiceException("Yahoo returned an invalid chart response.");

        if (chart.Error is not null || chart.Result is not { Count: > 0 })
        {
            throw new SymbolNotFoundException(RequestSymbol);
        }

        var result = chart.Result[0]
            ?? throw new UpstreamServiceException("Yahoo returned an invalid chart result.");

        var exchangeTimeZoneId = result.Meta?.ExchangeTimezoneName;
        if (string.IsNullOrWhiteSpace(exchangeTimeZoneId))
        {
            throw new UpstreamServiceException("Yahoo chart metadata did not include an exchange time zone.");
        }

        var candles = new List<Candle>();
        var timestamps = result.Timestamp;
        var symbol = result.Meta?.Symbol ?? RequestSymbol.ToUpperInvariant();
        var quote = result.Indicators?.Quote?.FirstOrDefault();
        var highs = quote?.High;
        var lows = quote?.Low;
        var volumes = quote?.Volume;
        //valid symbol with no intraday data is a valid response
        if (timestamps is not null &&
            highs is not null &&
            lows is not null &&
            volumes is not null)
        {
            var count = Math.Min(timestamps.Count, Math.Min(highs.Count, Math.Min(lows.Count, volumes.Count)));

            for (var index = 0; index < count; index++)
            {
                var timestamp = timestamps[index];
                var high = highs[index];
                var low = lows[index];
                var volume = volumes[index];

                if (timestamp is null || high is null || low is null || volume is null)
                {
                    continue;
                }

                try
                {
                    candles.Add(new Candle(
                        DateTimeOffset.FromUnixTimeSeconds(timestamp.Value),
                        high.Value,
                        low.Value,
                        volume.Value));
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new UpstreamServiceException(
                        "Yahoo returned a timestamp outside the supported range.",
                        exception);
                }
            }
        }

        return new ChartData(symbol, exchangeTimeZoneId, candles);
    }
}