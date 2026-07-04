using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.Login;
 
public sealed record LoginCommand(string Email, string Password, string? IpAddress) : ICommand<AuthTokensDto>;
 
public sealed record AuthTokensDto(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);