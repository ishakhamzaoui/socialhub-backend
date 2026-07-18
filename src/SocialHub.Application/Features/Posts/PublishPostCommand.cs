using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.6/6.7 — "publish now" from a Draft, or an early manual publish of something still Scheduled.</summary>
public sealed record PublishPostCommand(Guid PostId) : ICommand<PostDto>, IRequireAuthorization, ITransactionalRequest;