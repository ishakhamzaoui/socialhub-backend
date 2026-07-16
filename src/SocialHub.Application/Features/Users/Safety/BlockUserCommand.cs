using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed record BlockUserCommand(Guid TargetUserId) : ICommand, IRequireAuthorization, ITransactionalRequest;