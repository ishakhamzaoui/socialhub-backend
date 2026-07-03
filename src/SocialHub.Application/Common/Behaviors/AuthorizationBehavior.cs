using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Behaviors;
 
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ICurrentUserService _currentUserService;
 
    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
 
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequireAuthorization requireAuthorization)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return ResultFactory.CreateFailure<TResponse>(
                    Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required for this operation."));
            }
 
            var requiredRoles = requireAuthorization.Roles;
            if (requiredRoles is { Length: > 0 } && !requiredRoles.Any(role => _currentUserService.Roles.Contains(role)))
            {
                return ResultFactory.CreateFailure<TResponse>(
                    Error.Forbidden("Auth.Forbidden", "You do not have permission to perform this action."));
            }
        }
 
        return await next();
    }
}