using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private const int WarningThresholdMilliseconds = 500;
 
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
 
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
 
        if (stopwatch.ElapsedMilliseconds > WarningThresholdMilliseconds)
        {
            _logger.LogWarning(
                "Long-running request: {RequestName} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }
 
        return response;
    }
}