using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;

namespace backend.Middleware;


public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService, 
    ILogger<GlobalExceptionHandler> logger) :IExceptionHandler
{
    private const int ClientClosedRequestStatusCode = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && !context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request was canceled by the client.");
            context.Response.StatusCode = ClientClosedRequestStatusCode;
            return true;
        }

        var (status, title, detail) = exception switch
        {
            SymbolNotFoundException ex =>
                (StatusCodes.Status404NotFound, "Symbol not found", ex.Message),
            UpstreamServiceException ex =>
                (StatusCodes.Status502BadGateway, "Market data provider error", ex.Message),
            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred",
                    "Something went wrong. Please try again later.")
        };
        
        LogException(exception, status);

        context.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }

    private void LogException(Exception exception, int status)
    {
        switch (status)
        {
            case StatusCodes.Status404NotFound:
                logger.LogInformation("Symbol not found: {Message}", exception.Message);
                break;
            case StatusCodes.Status502BadGateway:
                logger.LogWarning(exception, "Upstream provider failure");
                break;
            default:
                logger.LogError(exception, "Unhandled exception");
                break;
        }
    }
    

}