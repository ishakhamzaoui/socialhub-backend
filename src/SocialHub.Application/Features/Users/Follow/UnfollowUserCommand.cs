using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed record UnfollowUserCommand(Guid TargetUserId) : ICommand, IRequireAuthorization, ITransactionalRequest;