using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.9. "At most one pinned post per author" is enforced in the handler, not Post itself — see this script's header.</summary>
public sealed record PinPostCommand(Guid PostId) : ICommand, IRequireAuthorization, ITransactionalRequest;