using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Common.Specifications;
using SocialHub.Application.Features.Authentication.Login;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Features.Authentication.RefreshToken;
 
public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthTokensDto>
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
 
    public RefreshTokenCommandHandler(
        ITokenService tokenService,
        IIdentityService identityService,
        IRepository<DomainRefreshToken, Guid> refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(request.RefreshToken);
        var existing = await _refreshTokens.FirstOrDefaultAsync(new RefreshTokenByHashSpecification(hash), cancellationToken);
 
        if (existing is null)
        {
            return Result.Failure<AuthTokensDto>(Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid."));
        }
 
        if (existing.IsRevoked)
        {
            // A revoked token being presented again signals possible theft —
            // revoke the whole family for this user as a precaution.
            var family = await _refreshTokens.ListAsync(new ActiveRefreshTokensByUserIdSpecification(existing.UserId), cancellationToken);
            foreach (var token in family)
            {
                token.Revoke(request.IpAddress);
            }
 
            await _unitOfWork.SaveChangesAsync(cancellationToken);
 
            return Result.Failure<AuthTokensDto>(Error.Unauthorized(
                "Auth.RefreshTokenReuseDetected",
                "This refresh token has already been used. All sessions for this account have been revoked as a precaution."));
        }
 
        if (existing.IsExpired)
        {
            return Result.Failure<AuthTokensDto>(Error.Unauthorized("Auth.RefreshTokenExpired", "The refresh token has expired. Please log in again."));
        }
 
        var userResult = await _identityService.GetUserByIdAsync(existing.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(userResult.Error);
        }
 
        var (rawRefreshToken, newHash) = _tokenService.GenerateRefreshToken();
        var expiresAtUtc = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime);
 
        existing.Revoke(request.IpAddress, newHash);
 
        // Carry the device name forward across rotation — same session, new token.
        var newRefreshToken = DomainRefreshToken.Create(existing.UserId, newHash, expiresAtUtc, request.IpAddress, existing.DeviceName);
        await _refreshTokens.AddAsync(newRefreshToken, cancellationToken);
 
        var accessToken = _tokenService.GenerateAccessToken(userResult.Value.Id, userResult.Value.Email, userResult.Value.Roles, userResult.Value.Permissions);
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success(new AuthTokensDto(accessToken, rawRefreshToken, expiresAtUtc));
    }
}