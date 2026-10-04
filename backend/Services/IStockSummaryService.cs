using backend.Models;

namespace backend.Services;

public interface IStockSummaryService
{
    Task<IReadOnlyList<DailySummaryDTO>> GetDailySummaryAsync(string symbol, CancellationToken ct);
}
