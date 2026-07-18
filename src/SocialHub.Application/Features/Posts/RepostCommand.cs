using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.11. Confirmed decision (script 31): a repost is a PostRepost row, not a Post — no content, no Media/Hashtags/Mentions of its own.</summary>
public sealed record RepostCommand(Guid OriginalPostId) : ICommand, IRequireAuthorization, ITransactionalRequest;