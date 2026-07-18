using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IPostRepository : IRepository<Post, Guid>
{
    /// <summary>Any post regardless of author, with Media/Hashtags/Mentions loaded. Used by GetPostQuery — call only after PostAccessPolicy has authorized the requester, same pattern as ProfileAccessPolicy + the unscoped MediaAsset lookup in Phase 5.</summary>
    Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
 
    /// <summary>Owner-scoped, with details loaded — the shape every mutating command (Update/Delete/Publish/Schedule/Archive/Pin) needs to confirm the requester actually owns the post before touching it.</summary>
    Task<Post?> GetByIdForAuthorAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default);
 
    /// <summary>The author's currently pinned post, if any. Used by PinPostCommandHandler to unpin the previous one first — "at most one pinned post per author" is enforced here, not on Post itself (a single Post can't know about its author's other posts).</summary>
    Task<Post?> GetPinnedPostAsync(Guid authorId, CancellationToken cancellationToken = default);
 
    /// <summary>The author's own posts (their drafts list, their scheduled queue, etc.), optionally filtered to a single status. Paginated the same shape as IFollowRepository's list methods.</summary>
    Task<(IReadOnlyList<Post> Posts, int TotalCount)> GetByAuthorAsync(Guid authorId, PostStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>Roadmap 6.7. Scheduled posts whose ScheduledForUtc has arrived — consumed by the scheduled-post publishing background job (added in a later script), which calls Post.Publish() on each result and saves.</summary>
    Task<IReadOnlyList<Post>> GetDuePostsForPublishingAsync(DateTime asOfUtc, CancellationToken cancellationToken = default);
}