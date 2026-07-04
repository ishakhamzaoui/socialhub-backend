using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.Sessions;
 
public sealed record RevokeSessionCommand(Guid SessionId) : ICommand, IRequireAuthorization;