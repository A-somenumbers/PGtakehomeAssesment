namespace backend.Models.Yahoo
{
    //mirrors the structure of the Yahoo Finance API response for chart data 
    //specifically the JSON returned by https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=15m&range=1mo
    public sealed record YahooChartResponse(YahooChart Chart);

    public sealed record YahooChart(
        IreadOnlyList<YahooChartResult> Result,
        YahooChartError? Error);

    public sealed record YahooChartResult(
        YahooMeta Meta,
        IReadOnlyList<long> Timestamp,
        YahooIndicators? Indicators);

    
    
}