using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
public sealed class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
 
    public AuditBehavior(IAuditService auditService, ICurrentUserService currentUserService)
    {
        _auditService = auditService;
        _currentUserService = currentUserService;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
 
        if (request is IAuditableRequest auditable && response.IsSuccess)
        {
            await _auditService.WriteAsync(
                new AuditEntry(auditable.ActionName, _currentUserService.UserId, DateTime.UtcNow),
                cancellationToken);
        }
 
        return response;
    }
}