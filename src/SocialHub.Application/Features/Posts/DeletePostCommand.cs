using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed record DeletePostCommand(Guid PostId) : ICommand, IRequireAuthorization;