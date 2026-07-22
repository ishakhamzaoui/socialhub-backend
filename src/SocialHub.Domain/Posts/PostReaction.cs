using SocialHub.Domain.Common;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 8 (Feed Engine) kickoff decision (script 51): Posts previously had
/// NO like/reaction mechanism at all — only Comments did (Phase 7's
/// CommentReaction). Rather than leave PostDto's new reaction-count field
/// with nothing to count, this entity is added now, deliberately mirroring
/// CommentReaction's shape exactly (see that type's remarks for the full
/// reasoning, repeated here only where it differs):
///
/// Relates two independent things (a User and a Post) — same shape as
/// Domain.Users.Follow / Domain.Posts.PostRepost / Domain.Comments
/// .CommentReaction, NOT an owned child of Post. A Post with thousands of
/// reactions must never drag all of them into memory just to build a
/// PostDto — its own repository aggregates counts-by-type directly in the
/// database instead.
///
/// One reaction per user per post: reacting again with a different type
/// calls ChangeType on the existing row rather than adding a second one —
/// enforced via a unique (PostId, UserId) index at the Persistence layer,
/// same belt-and-braces reasoning as CommentReactionConfiguration's index.
///
/// No domain event raised — no consumer exists yet (matches the
/// UserMute/PostRepost/CommentReaction precedent of not adding events
/// preemptively; Phase 11 can add one when reaction notifications are built).
/// </summary>
public sealed class PostReaction : BaseEntity, IAggregateRoot
{
    private PostReaction()
    {
        // Reserved for EF Core materialization.
    }
 
    private PostReaction(Guid id, Guid postId, Guid userId, ReactionType type)
        : base(id)
    {
        PostId = postId;
        UserId = userId;
        Type = type;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid PostId { get; private set; }
 
    public Guid UserId { get; private set; }
 
    public ReactionType Type { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public DateTime? UpdatedAtUtc { get; private set; }
 
    public static PostReaction Create(Guid postId, Guid userId, ReactionType type) =>
        new(Guid.NewGuid(), postId, userId, type);
 
    public void ChangeType(ReactionType type)
    {
        Type = type;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}