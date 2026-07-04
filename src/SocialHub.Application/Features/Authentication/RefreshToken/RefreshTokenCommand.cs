using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Features.Authentication.Login;
 
namespace SocialHub.Application.Features.Authentication.RefreshToken;
 
public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : ICommand<AuthTokensDto>;