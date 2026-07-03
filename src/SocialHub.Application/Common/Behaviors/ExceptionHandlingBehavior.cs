using MediatR;
using Microsoft.Extensions.Logging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
/// <summary>
/// Outermost behavior. Converts any unhandled exception thrown further down
/// the pipeline into a failed Result, so callers never have to catch
/// exceptions from Send(); genuinely unexpected failures (e.g. infrastructure
/// outages) still surface as a generic 500-equivalent failure here.
/// </summary>
public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
 
    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {RequestName}", typeof(TRequest).Name);
 
            return ResultFactory.CreateFailure<TResponse>(
                Error.Failure("Server.Unhandled", "An unexpected error occurred while processing the request."));
        }
    }
}