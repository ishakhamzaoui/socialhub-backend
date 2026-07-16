using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Safety;
 
/// <summary>Does not restore any follow relationship that BlockUserCommand removed — unblocking is not un-following's inverse.</summary>
public sealed record UnblockUserCommand(Guid TargetUserId) : ICommand, IRequireAuthorization, ITransactionalRequest;