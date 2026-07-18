using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>
/// Roadmap 6.1, 6.5 (media), 6.6/6.7 (draft/scheduled via DesiredStatus),
/// 6.10 (quote), 6.12 (hashtags), 6.13 (mentions). DesiredStatus is never
/// Archived here — a post can't be created pre-archived. HashtagTags and
/// MentionedUserIds are explicit client-supplied lists, not parsed from
/// Content (confirmed decision, this phase's kickoff — see script header).
/// </summary>
public sealed record CreatePostCommand(
    string? Content,
    PostVisibility Visibility,
    PostType Type,
    Guid? OriginalPostId,
    PostStatus DesiredStatus,
    DateTime? ScheduledForUtc,
    IReadOnlyList<Guid>? MediaAssetIds,
    IReadOnlyList<string>? HashtagTags,
    IReadOnlyList<Guid>? MentionedUserIds) : ICommand<PostDto>, IRequireAuthorization, ITransactionalRequest;