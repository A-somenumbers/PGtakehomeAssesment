# Prompt Log

## 2026-10-04

- From this point on, keep a prompt_log.md of everything I ask
- Do not start work on anything yet, but for this project we are building a full stack app. For the backend, we are building a self hosted C# .NET 9 Web API that uses "https://query1.finance.yahoo.com/" for our data. Through this we expose an endpoint that: takes a stock symbol as a parameter, queries intraday data from the past month, groups these results by day, and returns JSON in the following format. Keep this in memory.
- Still don't do anything yet; this is just for memory. For the frontend, we are building a React UI that consumes the backend, lets the user enter a stock symbol, displays results in a table or chart, and handles errors such as invalid symbols and failed requests.
- Ground rules: work in smaller steps and test before moving on; never fabricate results; use decimal for prices and long for volume; pass CancellationToken ct through every async method.
- Start with the backend layout; create folders for clients, endpoints, models, services, options, middleware, and exceptions.
- In backend/Models/Candle.cs, create a candle object that contains a DateTimeOffset, decimal high and low, and long volume.
- In the backend models, create a chart data object containing a string Symbol, a string ExchangeTimeZoneID, and a read-only list of candles.
- Change the two model types, Candle and ChartData, to sealed records.
- In backend/appsettings.json, add a reference to "https://query1.finance.yahoo.com/" labeled "yahoo".
- In backend/Options, add YahooOptions.cs containing the Yahoo URL and settings.
- In backend/Exceptions/StockExceptions.cs, add a SymbolNotFoundException with a Symbol property mapped to 404, and an UpstreamServiceException mapped to 502. Higher layers must not receive HttpRequestException, JsonException, or timeout exceptions directly.
