using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.8. Flagged assumption (script 42 header item 7): this is a
/// MINIMAL, comment-specific report record — who reported which comment,
/// why (CommentReportReason), and optional free-text Details. There is
/// deliberately no Status/queue/workflow field here: Phase 14 (Moderation)
/// is where the general cross-entity Reports/queue/appeals system gets
/// designed for posts/comments/users/communities alike, and this script
/// does not want to unilaterally pre-decide that shape.
///
/// Relates two independent things (a User and a Comment) — own aggregate +
/// own repository, same shape as CommentReaction/PostRepost/Follow, not an
/// owned child of Comment.
///
/// No domain event raised — matches the UserMute/PostRepost restraint
/// precedent (spec §16's catalog has no "CommentReported" entry, and
/// nothing consumes one yet; Phase 14 can add it when the moderation queue
/// exists to receive it).
/// </summary>
public sealed class CommentReport : BaseEntity, IAggregateRoot
{
    private CommentReport()
    {
        // Reserved for EF Core materialization.
    }
 
    private CommentReport(Guid id, Guid commentId, Guid reporterId, CommentReportReason reason, string? details)
        : base(id)
    {
        CommentId = commentId;
        ReporterId = reporterId;
        Reason = reason;
        Details = details;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid CommentId { get; private set; }
 
    public Guid ReporterId { get; private set; }
 
    public CommentReportReason Reason { get; private set; }
 
    public string? Details { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static CommentReport Create(Guid commentId, Guid reporterId, CommentReportReason reason, string? details = null) =>
        new(Guid.NewGuid(), commentId, reporterId, reason, details);
}