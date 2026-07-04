using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Common.Specifications;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Features.Authentication.Logout;
 
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ITokenService _tokenService;
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
 
    public LogoutCommandHandler(
        ITokenService tokenService,
        IRepository<DomainRefreshToken, Guid> refreshTokens,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }
 
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(request.RefreshToken);
        var existing = await _refreshTokens.FirstOrDefaultAsync(new RefreshTokenByHashSpecification(hash), cancellationToken);
 
        if (existing is null)
        {
            // Already gone / never existed — logout is idempotent from the caller's perspective.
            return Result.Success();
        }
 
        if (_currentUserService.UserId is null || existing.UserId.ToString() != _currentUserService.UserId)
        {
            return Result.Failure(Error.Forbidden("Auth.TokenOwnershipMismatch", "This refresh token does not belong to the current user."));
        }
 
        if (existing.IsActive)
        {
            existing.Revoke(null);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
 
        return Result.Success();
    }
}