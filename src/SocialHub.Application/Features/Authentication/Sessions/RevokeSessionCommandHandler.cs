using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Features.Authentication.Sessions;
 
public sealed class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, Result>
{
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
 
    public RevokeSessionCommandHandler(
        IRepository<DomainRefreshToken, Guid> refreshTokens,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }
 
    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null || !Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Not authenticated."));
        }
 
        var token = await _refreshTokens.GetByIdAsync(request.SessionId, cancellationToken);
        if (token is null || token.UserId != userId)
        {
            return Result.Failure(Error.NotFound("Auth.SessionNotFound", "Session not found."));
        }
 
        if (token.IsActive)
        {
            token.Revoke(null);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
 
        return Result.Success();
    }
}