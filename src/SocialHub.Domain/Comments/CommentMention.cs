using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.7. A single explicit mention on a comment. Owned by the
/// Comment aggregate, same rationale as Domain.Posts.PostMention: no
/// independent lifecycle, only ever reached through its one Comment.
/// MentionedUserId is a bare Guid (ApplicationUser.Id) — Domain cannot
/// reference SocialHub.Identity. Confirmed decision (Phase 7 kickoff, item
/// 4): explicit client-supplied MentionedUserIds, never parsed from
/// Content — no username/handle concept exists anywhere in this codebase
/// to resolve @text against (same reasoning as Phase 6's PostMention).
/// </summary>
public sealed class CommentMention : BaseEntity
{
    private CommentMention()
    {
        // Reserved for EF Core materialization.
    }
 
    private CommentMention(Guid id, Guid commentId, Guid mentionedUserId)
        : base(id)
    {
        CommentId = commentId;
        MentionedUserId = mentionedUserId;
    }
 
    public Guid CommentId { get; private set; }
 
    public Guid MentionedUserId { get; private set; }
 
    internal static CommentMention Create(Guid commentId, Guid mentionedUserId) =>
        new(Guid.NewGuid(), commentId, mentionedUserId);
}