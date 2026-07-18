using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed record UnpinPostCommand(Guid PostId) : ICommand, IRequireAuthorization, ITransactionalRequest;