using backend.Endpoints;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace backend.Tests.Endpoints;

public sealed class StockEndpointsTests
{
    [Fact]
    public async Task GetDailySummaryAsync_RejectsInvalidSymbolWithoutCallingService()
    {
        var service = new StubStockSummaryService();
        using var cancellationSource = new CancellationTokenSource();

        var result = await StockEndpoints.GetDailySummaryAsync(
            "AAPL/$",
            service,
            cancellationSource.Token);

        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.Value!.Status);
        Assert.False(service.WasCalled);
    }

    [Fact]
    public async Task GetDailySummaryAsync_DelegatesAndReturnsOkResult()
    {
        IReadOnlyList<DailySummaryDTO> summaries =
        [
            new DailySummaryDTO(new DateOnly(2026, 10, 2), 10m, 12m, 100L)
        ];
        var service = new StubStockSummaryService(summaries);
        using var cancellationSource = new CancellationTokenSource();

        var result = await StockEndpoints.GetDailySummaryAsync(
            "  aapl  ",
            service,
            cancellationSource.Token);

        var ok = Assert.IsType<Ok<IReadOnlyList<DailySummaryDTO>>>(result.Result);
        Assert.Same(summaries, ok.Value);
        Assert.True(service.WasCalled);
        Assert.Equal("aapl", service.ReceivedSymbol);
        Assert.Equal(cancellationSource.Token, service.ReceivedCancellationToken);
    }

    private sealed class StubStockSummaryService(
        IReadOnlyList<DailySummaryDTO>? summaries = null) : IStockSummaryService
    {
        private readonly IReadOnlyList<DailySummaryDTO> _summaries =
            summaries ?? Array.Empty<DailySummaryDTO>();

        public bool WasCalled { get; private set; }
        public string? ReceivedSymbol { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<IReadOnlyList<DailySummaryDTO>> GetDailySummaryAsync(
            string symbol,
            CancellationToken ct)
        {
            WasCalled = true;
            ReceivedSymbol = symbol;
            ReceivedCancellationToken = ct;
            return Task.FromResult(_summaries);
        }
    }
}
