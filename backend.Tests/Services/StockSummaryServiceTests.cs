using backend.Clients;
using backend.Exceptions;
using backend.Models;
using backend.Services;

namespace backend.Tests.Services;

public sealed class StockSummaryServiceTests
{
    [Fact]
    public async Task GetDailySummaryAsync_ReturnsEmptyWhenThereAreNoCandles()
    {
        var client = new StubYahooFinanceClient(
            new ChartData("AAPL", "Unknown/Timezone", Array.Empty<Candle>()));
        var service = new StockSummaryService(client);
        using var cancellationSource = new CancellationTokenSource();

        var summaries = await service.GetDailySummaryAsync("AAPL", cancellationSource.Token);

        Assert.Empty(summaries);
        Assert.Equal("AAPL", client.RequestedSymbol);
        Assert.Equal(cancellationSource.Token, client.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetDailySummaryAsync_GroupsByExchangeDateSortsAggregatesAndRounds()
    {
        var client = new StubYahooFinanceClient(new ChartData(
            "AAPL",
            "America/New_York",
            [
                new Candle(
                    new DateTimeOffset(2024, 1, 3, 15, 0, 0, TimeSpan.Zero),
                    30.12349m,
                    20.12349m,
                    300L),
                new Candle(
                    new DateTimeOffset(2024, 1, 2, 15, 0, 0, TimeSpan.Zero),
                    12.11111m,
                    10.11111m,
                    100L),
                new Candle(
                    new DateTimeOffset(2024, 1, 2, 16, 0, 0, TimeSpan.Zero),
                    14.22222m,
                    12.22222m,
                    200L)
            ]));
        var service = new StockSummaryService(client);
        using var cancellationSource = new CancellationTokenSource();

        var summaries = await service.GetDailySummaryAsync("AAPL", cancellationSource.Token);

        Assert.Collection(
            summaries,
            first =>
            {
                Assert.Equal(new DateOnly(2024, 1, 2), first.Date);
                Assert.Equal(11.1667m, first.LowAverage);
                Assert.Equal(13.1667m, first.HighAverage);
                Assert.Equal(300L, first.TotalVolume);
            },
            second =>
            {
                Assert.Equal(new DateOnly(2024, 1, 3), second.Date);
                Assert.Equal(20.1235m, second.LowAverage);
                Assert.Equal(30.1235m, second.HighAverage);
                Assert.Equal(300L, second.TotalVolume);
            });
    }

    [Fact]
    public async Task GetDailySummaryAsync_ThrowsUpstreamExceptionForUnknownTimezone()
    {
        var client = new StubYahooFinanceClient(new ChartData(
            "AAPL",
            "Unknown/Timezone",
            [new Candle(DateTimeOffset.UnixEpoch, 1m, 1m, 1L)]));
        var service = new StockSummaryService(client);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(
            () => service.GetDailySummaryAsync("AAPL", cancellationSource.Token));

        Assert.IsType<TimeZoneNotFoundException>(exception.InnerException);
    }

    private sealed class StubYahooFinanceClient(ChartData chartData) : IYahooFinanceClient
    {
        public string? RequestedSymbol { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<ChartData> GetIntradayAsync(string symbol, CancellationToken ct)
        {
            RequestedSymbol = symbol;
            ReceivedCancellationToken = ct;
            return Task.FromResult(chartData);
        }
    }
}
