using MediatR;
using Microsoft.Extensions.Logging;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
 
    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
        {
            return await next();
        }
 
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
 
        try
        {
            var response = await next();
 
            if (response.IsSuccess)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            else
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
            }
 
            return response;
        }
        catch
        {
            _logger.LogWarning("Rolling back transaction for {RequestName} due to an exception", typeof(TRequest).Name);
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}