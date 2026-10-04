using backend.Clients;
using backend.Exceptions;
using backend.Models;

namespace backend.Services;

public sealed class StockSummaryService(IYahooFinanceClient yahooFinanceClient) : IStockSummaryService
{
    public async Task<IReadOnlyList<DailySummaryDTO>> GetDailySummaryAsync(
        string symbol,
        CancellationToken ct)
    {
        var chartData = await yahooFinanceClient.GetIntradayAsync(symbol, ct);
        if (chartData.Candles.Count == 0)
        {
            return Array.Empty<DailySummaryDTO>();
        }

        TimeZoneInfo exchangeTimeZone;
        try
        {
            exchangeTimeZone = TimeZoneInfo.FindSystemTimeZoneById(chartData.ExchangeTimeZoneID);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new UpstreamServiceException(
                $"Yahoo returned an unrecognized exchange time zone '{chartData.ExchangeTimeZoneID}'.",
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new UpstreamServiceException(
                $"Yahoo returned an invalid exchange time zone '{chartData.ExchangeTimeZoneID}'.",
                exception);
        }

        var summaries = chartData.Candles
            .GroupBy(candle => DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(candle.Time, exchangeTimeZone).DateTime))
            .OrderBy(group => group.Key)
            .Select(group => new DailySummaryDTO(
                group.Key,
                Math.Round(group.Average(candle => candle.Low), 4),
                Math.Round(group.Average(candle => candle.High), 4),
                group.Sum(candle => candle.Volume)))
            .ToArray();

        return summaries;
    }
}