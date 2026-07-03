using MediatR;
using Microsoft.Extensions.Logging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
 
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();
 
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestName"] = requestName
        });
 
        _logger.LogInformation("Handling {RequestName} ({RequestId})", requestName, requestId);
 
        var response = await next();
 
        if (response is { IsSuccess: false } failed)
        {
            _logger.LogWarning(
                "{RequestName} ({RequestId}) failed: {ErrorCode} - {ErrorMessage}",
                requestName, requestId, failed.Error.Code, failed.Error.Message);
        }
        else
        {
            _logger.LogInformation("Handled {RequestName} ({RequestId})", requestName, requestId);
        }
 
        return response;
    }
}