using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.5/7.6. Flagged assumption (script 42 header item 6): Likes and
/// Reactions are ONE unified mechanism — this row's Type is Like, Love,
/// Laugh, Sad, or Angry, with Like being just one of the values rather than
/// a separate boolean concept.
///
/// Relates two independent things (a User and a Comment) — same shape as
/// Domain.Users.Follow / Domain.Posts.PostRepost, NOT an owned child of
/// Comment. Reasoning: an owned-child-collection would force loading every
/// reaction row whenever a Comment loads, which defeats fast reaction
/// counts (a comment with thousands of reactions would drag all of them
/// into memory just to build a DTO). Its own repository can aggregate
/// counts-by-type directly in the database instead.
///
/// One reaction per user per comment: reacting again with a different type
/// calls ChangeType on the existing row rather than adding a second one —
/// enforced via a unique (CommentId, UserId) index at the Persistence layer
/// plus the handler looking up any existing row first (same
/// look-up-then-decide shape as BlockUserCommandHandler's existing-block
/// check).
///
/// No domain event raised — no Phase 7 consumer exists yet (matches the
/// UserMute/PostRepost precedent of not adding events preemptively; Phase
/// 11 can add one when reaction notifications are built).
/// </summary>
public sealed class CommentReaction : BaseEntity, IAggregateRoot
{
    private CommentReaction()
    {
        // Reserved for EF Core materialization.
    }
 
    private CommentReaction(Guid id, Guid commentId, Guid userId, ReactionType type)
        : base(id)
    {
        CommentId = commentId;
        UserId = userId;
        Type = type;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid CommentId { get; private set; }
 
    public Guid UserId { get; private set; }
 
    public ReactionType Type { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public DateTime? UpdatedAtUtc { get; private set; }
 
    public static CommentReaction Create(Guid commentId, Guid userId, ReactionType type) =>
        new(Guid.NewGuid(), commentId, userId, type);
 
    public void ChangeType(ReactionType type)
    {
        Type = type;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}