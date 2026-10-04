using backend.Models;
using backend.Services;


namespace backend.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/stocks").WithTags("Stocks");
        group.MapGet("/{symbol}/daily-summary", GetDailySummary)
            .WithName("GetStockDailySummary")
            .WithSummary("Get daily trading summaries for a stock symbol")
            .Produces<IReadOnlyList<DailySummaryDTO>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return endpoints;
    }

    internal static async Task<IResult> GetDailySummary(
        string symbol,
        IStockSummaryService stockSummaryService,
        CancellationToken ct)
    {
        var normalized = symbol.Trim().ToUpperInvariant();

        if (!IsValidSymbol(normalized))
        {
             return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["symbol"] = ["Symbol must be 1-10 characters: letters, digits, '.', '-', '^' or '='."]
            });
        }

        var summaries = await stockSummaryService.GetDailySummaryAsync(normalized, ct);

        return TypedResults.Ok(summaries);
    }

    private static bool IsValidSymbol(string symbol)
    {
        var trimmedSymbol = symbol.Trim();
        return trimmedSymbol.Length is > 0 and <= 32 &&
               trimmedSymbol.All(character =>
                   char.IsAsciiLetterOrDigit(character) ||
                   character is '.' or '^' or '=' or '_' or '-');
    }
}