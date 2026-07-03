using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
 
namespace SocialHub.API.Middleware;
 
/// <summary>
/// Catches any exception that escapes the MediatR pipeline (e.g. model
/// binding, routing, or middleware failures) and converts it into an
/// RFC 7807 ProblemDetails response. Application-layer exceptions are
/// already converted to failed Results by ExceptionHandlingBehavior and
/// never reach this handler.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
 
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
 
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
 
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.io/500",
            Detail = "An unexpected error occurred while processing your request.",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
 
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
 
        return true;
    }
}