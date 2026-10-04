using System.Net;
using System.Text.Json;
using backend.Exceptions;
using backend.Models;
using backend.Models.Yahoo;
using backend.Options;
using Microsoft.Extensions.Options;

namespace backend.Clients;

public sealed class YahooFinanceClient : IYahooFinanceClient
{
    private readonly HttpClient _httpClient;
    private readonly YahooOptions _options;

    public YahooFinanceClient(
        HttpClient httpClient,
        IOptions<YahooOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<ChartData> GetIntradayAsync(string symbol, CancellationToken ct)
    {
        var requestUri =
            $"v8/finance/chart/{Uri.EscapeDataString(symbol)}" +
            $"?interval={Uri.EscapeDataString(_options.Interval)}" +
            $"&range={Uri.EscapeDataString(_options.Range)}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                ct);
        }
        catch (HttpRequestException exception)
        {
            throw new UpstreamServiceException("Yahoo Finance could not be reached.", exception);
        }
        catch (OperationCanceledException exception) when (!ct.IsCancellationRequested)
        {
            throw new UpstreamServiceException("Yahoo Finance request timed out.", exception);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new SymbolNotFoundException(symbol);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new UpstreamServiceException(
                    $"Yahoo Finance returned HTTP status {(int)response.StatusCode} for symbol '{symbol}'.");
            }

            YahooChartResponse? yahooResponse;
            try
            {
                yahooResponse = await response.Content.ReadFromJsonAsync<YahooChartResponse>(
                    cancellationToken: ct);
            }
            catch (JsonException exception)
            {
                throw new UpstreamServiceException(
                    $"Yahoo Finance returned malformed JSON for symbol '{symbol}'.",
                    exception);
            }
            catch (HttpRequestException exception)
            {
                throw new UpstreamServiceException(
                    $"Reading the Yahoo Finance response failed for symbol '{symbol}'.",
                    exception);
            }
            catch (OperationCanceledException exception) when (!ct.IsCancellationRequested)
            {
                throw new UpstreamServiceException(
                    $"Reading the Yahoo Finance response timed out for symbol '{symbol}'.",
                    exception);
            }

            if (yahooResponse is null)
            {
                throw new UpstreamServiceException(
                    $"Yahoo Finance returned an empty response body for symbol '{symbol}'.");
            }

            return YahooChartMapper.Map(symbol, yahooResponse);
        }
    }
}