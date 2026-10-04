using backend.Clients;
using backend.Endpoints;
using backend.Middleware;
using backend.Options;
using backend.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


// configure services
builder.Services
.AddOptions<YahooOptions>()
.BindConfiguration(YahooOptions.SectionName)
.ValidateDataAnnotations()
.ValidateOnStart();

//upstream client
builder.Services.Configure<YahooOptions>(
    builder.Configuration.GetSection(YahooOptions.SectionName));
builder.Services
.AddHttpClient<IYahooFinanceClient, YahooFinanceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<YahooOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

//application sercvices
builder.Services.AddScoped<IStockSummaryService, StockSummaryService>();

//error handling middleware
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//cors
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithMethods(HttpMethods.Get)
            .AllowAnyHeader();
    });
});


builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapStockEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;// for integration testing
