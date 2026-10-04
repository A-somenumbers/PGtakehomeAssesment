using System.Net;
using backend.Clients;
using backend.Exceptions;
using backend.Models.Yahoo;
using backend.Options;
using Microsoft.Extensions.Options;

namespace backend.Tests.Clients;

public sealed class YahooFinanceClientTests
{
    [Fact]
    public async Task GetIntradayAsync_BuildsRequestAndMapsSuccessfulResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        using var httpClient = CreateHttpClient((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(JsonResponse(SuccessfulResponse));
        });
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var chartData = await client.GetIntradayAsync("AAPL", cancellationSource.Token);

        Assert.NotNull(capturedRequest);
        Assert.Equal(
            "/v8/finance/chart/AAPL",
            capturedRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?interval=15m&range=1mo", capturedRequest.RequestUri.Query);
        Assert.Equal("AAPL", chartData.Symbol);
        Assert.Single(chartData.Candles);
    }

    [Fact]
    public async Task GetIntradayAsync_MapsHttpNotFoundToSymbolNotFound()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<SymbolNotFoundException>(
            () => client.GetIntradayAsync("MISSING", cancellationSource.Token));

        Assert.Equal("MISSING", exception.Symbol);
    }

    [Fact]
    public async Task GetIntradayAsync_MapsOtherHttpFailuresToUpstreamException()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));

        Assert.Contains("503", exception.Message);
    }

    [Fact]
    public async Task GetIntradayAsync_MapsMalformedJsonToUpstreamException()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{malformed")
            }));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));

        Assert.IsType<System.Text.Json.JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task GetIntradayAsync_MapsEmptyBodyToUpstreamException()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            }));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        await Assert.ThrowsAsync<UpstreamServiceException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));
    }

    [Fact]
    public async Task GetIntradayAsync_MapsHttpRequestExceptionToUpstreamException()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Network failure")));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));

        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task GetIntradayAsync_MapsTimeoutToUpstreamException()
    {
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromException<HttpResponseMessage>(
                new OperationCanceledException("Timed out")));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));

        Assert.Contains("timed out", exception.Message);
    }

    [Fact]
    public async Task GetIntradayAsync_PropagatesCallerCancellation()
    {
        using var httpClient = CreateHttpClient((_, ct) =>
            Task.FromCanceled<HttpResponseMessage>(ct));
        var client = CreateClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetIntradayAsync("AAPL", cancellationSource.Token));
    }

    private static YahooFinanceClient CreateClient(HttpClient httpClient) =>
        new(httpClient, Microsoft.Extensions.Options.Options.Create(new YahooOptions
        {
            BaseUrl = "https://query1.finance.yahoo.com/",
            Interval = "15m",
            Range = "1mo"
        }));

    private static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) =>
        new(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("https://query1.finance.yahoo.com/")
        };

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

    private const string SuccessfulResponse =
        """
        {
          "chart": {
            "result": [{
              "meta": {
                "symbol": "AAPL",
                "exchangeTimezoneName": "America/New_York"
              },
              "timestamp": [1728000000],
              "indicators": {
                "quote": [{
                  "high": [101.25],
                  "low": [99.75],
                  "volume": [123456]
                }]
              }
            }],
            "error": null
          }
        }
        """;

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            sendAsync(request, cancellationToken);
    }
}
