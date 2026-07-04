using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Authentication.Login;
 
public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthTokensDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IRepository<Domain.Users.RefreshToken, Guid> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
 
    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IRepository<Domain.Users.RefreshToken, Guid> refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validation = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (validation.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(validation.Error);
        }
 
        var user = validation.Value;
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.Permissions);
        var (rawRefreshToken, refreshTokenHash) = _tokenService.GenerateRefreshToken();
        var expiresAtUtc = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime);
 
        var refreshToken = Domain.Users.RefreshToken.Create(user.Id, refreshTokenHash, expiresAtUtc, request.IpAddress, request.DeviceName);
        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success(new AuthTokensDto(accessToken, rawRefreshToken, expiresAtUtc));
    }
}