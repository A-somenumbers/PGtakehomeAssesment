using backend.Clients;
using backend.Exceptions;
using backend.Models.Yahoo;

namespace backend.Tests.Clients;

public sealed class YahooChartMapperTests
{
    [Fact]
    public void Map_ConvertsUnixSecondsAndMapsYahooSymbolAndTimezone()
    {
        var response = CreateResponse(
            timestamps: [1_728_000_000],
            highs: [101.25m],
            lows: [99.75m],
            volumes: [123_456L],
            responseSymbol: "AAPL");

        var result = YahooChartMapper.Map("aapl", response);

        Assert.Equal("AAPL", result.Symbol);
        Assert.Equal("America/New_York", result.ExchangeTimeZoneID);
        var candle = Assert.Single(result.Candles);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1_728_000_000), candle.Time);
        Assert.Equal(101.25m, candle.High);
        Assert.Equal(99.75m, candle.Low);
        Assert.Equal(123_456L, candle.Volume);
    }

    [Fact]
    public void Map_ThrowsSymbolNotFoundWhenYahooReturnsError()
    {
        var response = new YahooChartResponse(
            new YahooChart(null, new YahooError("Not Found", "No data found")));

        var exception = Assert.Throws<SymbolNotFoundException>(
            () => YahooChartMapper.Map("MISSING", response));

        Assert.Equal("MISSING", exception.Symbol);
    }

    [Fact]
    public void Map_ThrowsSymbolNotFoundWhenResultIsEmpty()
    {
        var response = new YahooChartResponse(new YahooChart([], null));

        var exception = Assert.Throws<SymbolNotFoundException>(
            () => YahooChartMapper.Map("MISSING", response));

        Assert.Equal("MISSING", exception.Symbol);
    }

    [Fact]
    public void Map_ThrowsUpstreamExceptionWhenTimezoneIsMissing()
    {
        var response = CreateResponse(timezone: null);

        Assert.Throws<UpstreamServiceException>(
            () => YahooChartMapper.Map("AAPL", response));
    }

    [Fact]
    public void Map_ReturnsEmptyCandlesWhenArraysAreMissing()
    {
        var response = CreateResponse(timestamps: null, highs: null, lows: null, volumes: null);

        var result = YahooChartMapper.Map("AAPL", response);

        Assert.Empty(result.Candles);
    }

    [Fact]
    public void Map_UsesShortestArrayAndSkipsIncompleteCandles()
    {
        var response = CreateResponse(
            timestamps: [1L, 2L, 3L],
            highs: [10m, null, 30m, 40m],
            lows: [5m, 15m, null],
            volumes: [100L, 200L, 300L, 400L]);

        var result = YahooChartMapper.Map("AAPL", response);

        var candle = Assert.Single(result.Candles);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1), candle.Time);
        Assert.Equal(10m, candle.High);
        Assert.Equal(5m, candle.Low);
        Assert.Equal(100L, candle.Volume);
    }

    [Fact]
    public void Map_ThrowsUpstreamExceptionForOutOfRangeTimestamp()
    {
        var response = CreateResponse(
            timestamps: [long.MaxValue],
            highs: [10m],
            lows: [5m],
            volumes: [100L]);

        Assert.Throws<UpstreamServiceException>(
            () => YahooChartMapper.Map("AAPL", response));
    }

    private static YahooChartResponse CreateResponse(
        IReadOnlyList<long?>? timestamps = null,
        IReadOnlyList<decimal?>? highs = null,
        IReadOnlyList<decimal?>? lows = null,
        IReadOnlyList<long?>? volumes = null,
        string? responseSymbol = "AAPL",
        string? timezone = "America/New_York")
    {
        return new YahooChartResponse(
            new YahooChart(
                [
                    new YahooChartResult(
                        new YahooMeta(responseSymbol, timezone),
                        timestamps,
                        new YahooIndicators([new YahooQuote(highs, lows, volumes)]))
                ],
                null));
    }
}
