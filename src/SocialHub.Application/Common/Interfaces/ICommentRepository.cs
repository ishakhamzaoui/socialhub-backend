using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface ICommentRepository : IRepository<Comment, Guid>
{
    /// <summary>
    /// Any comment regardless of author, with Mentions loaded. Used by
    /// GetCommentQuery — call only after the parent Post has been checked
    /// against PostAccessPolicy, same "resolve access first, then read"
    /// pattern GetPostQuery already established for GetByIdWithDetailsAsync.
    /// </summary>
    Task<Comment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Author-scoped, with Mentions loaded — the shape UpdateComment/
    /// DeleteComment need to confirm the requester actually owns the
    /// comment before mutating it. (Pin/Unpin use a different ownership
    /// check — the POST's author, not necessarily the comment's author, is
    /// who's allowed to pin — see PinCommentCommandHandler's remarks.)
    /// </summary>
    Task<Comment?> GetByIdForAuthorAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Top-level comments on a post (ParentCommentId == null), paginated.
    /// Soft-deleted comments are NOT filtered out — they still occupy their
    /// place in the thread as tombstones (Content == null), same reasoning
    /// as Comment.MarkDeleted()'s remarks. Ordering: pinned comment first
    /// (if any), then chronological.
    ///
    /// excludeAuthorIds (added in script 45's correction — see this
    /// script's header): applied inside the EF query, same "keep pagination
    /// totals accurate" reasoning as IFollowRepository's excludeUserIds
    /// (Phase 5) — filtering after the fact would silently understate
    /// TotalCount whenever a block is involved.
    /// </summary>
    Task<(IReadOnlyList<Comment> Comments, int TotalCount)> GetTopLevelForPostAsync(Guid postId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeAuthorIds = null, CancellationToken cancellationToken = default);
 
    /// <summary>Replies to a specific parent comment, paginated, chronological. Same soft-delete-not-filtered and excludeAuthorIds reasoning as GetTopLevelForPostAsync.</summary>
    Task<(IReadOnlyList<Comment> Replies, int TotalCount)> GetRepliesAsync(Guid parentCommentId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeAuthorIds = null, CancellationToken cancellationToken = default);
 
    /// <summary>Cheap reply count for a comment (e.g. "12 replies" without loading them) — a straight COUNT, not GetRepliesAsync's full paginated fetch.</summary>
    Task<int> GetReplyCountAsync(Guid parentCommentId, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// The post's currently pinned comment, if any. Mirrors
    /// IPostRepository.GetPinnedPostAsync exactly — used by
    /// PinCommentCommandHandler to unpin the previous one first, since a
    /// single Comment can't know about its post's other comments.
    /// </summary>
    Task<Comment?> GetPinnedCommentForPostAsync(Guid postId, CancellationToken cancellationToken = default);
}