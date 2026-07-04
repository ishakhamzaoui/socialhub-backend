using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.Logout;
 
public sealed record LogoutCommand(string RefreshToken) : ICommand, IRequireAuthorization;