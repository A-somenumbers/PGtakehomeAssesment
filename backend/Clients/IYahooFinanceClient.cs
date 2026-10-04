using backend.Models;

namespace backend.Clients
{
    public interface IYahooFinanceClient
    {
        Task<ChartData> GetIntradayAsync(string symbol, CancellationToken ct);
    }
}