using SocialHub.Application.Common.Pagination;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// One row in a feed result: either an original/quote Post authored by
/// AuthorId-implicit-in-the-Post-itself (RepostedByUserId is null), or a
/// repost of PostId by RepostedByUserId. FeedTimestampUtc is always the
/// "when this appeared in the feed" value (the post's CreatedAtUtc for a
/// direct entry, the repost's CreatedAtUtc for a repost entry) — used for
/// display regardless of feed type. SortKey is what the feed actually
/// orders/pages by, which is NOT always FeedTimestampUtc.Ticks — see
/// GetTrendingFeedAsync's remarks.
/// </summary>
public sealed record FeedEntry(Guid PostId, Guid? RepostedByUserId, DateTime FeedTimestampUtc, long SortKey);
 
/// <summary>
/// See script 55's header for why this is a cross-aggregate repository
/// (Post + PostRepost + Follow + UserBlock) rather than living on
/// IPostRepository or IPostRepostRepository. All four methods apply block
/// and visibility filtering inside the query itself (never post-hoc), per
/// this project's established excludeUserIds/excludeAuthorIds convention.
/// </summary>
public interface IFeedRepository
{
    /// <summary>Roadmap 8.1 — posts and reposts from users the requester follows. Excludes the requester's own posts (this is "people I follow," not "my own timeline" — GetMyPostsQuery already covers that).</summary>
    Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetFollowingFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>Roadmap 8.2 — the platform-wide public timeline (all visible posts/reposts, not limited to who the requester follows), chronological.</summary>
    Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetChronologicalFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Roadmap 8.3 — confirmed "keep it simple" design: ranked by repost
    /// count within a recent window (see FeedRepository's TrendingWindow),
    /// then recency as a tiebreak. No comment/reaction engagement signal
    /// yet (confirmed deferred — Phase 13+ territory).
    /// </summary>
    Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetTrendingFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Roadmap 8.4 — confirmed design: the following feed plus posts from
    /// the requester's suggested users mixed in. suggestedUserIds is
    /// resolved by the CALLER via IFollowRepository.GetSuggestedUserIdsAsync
    /// (Phase 5's mutual-followers logic) — this repository doesn't
    /// duplicate that logic, it just accepts the resulting id list so the
    /// two concerns (who's suggested vs. what's visible) stay separate.
    /// </summary>
    Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetPersonalizedFeedAsync(Guid requesterId, IReadOnlyList<Guid> suggestedUserIds, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default);
}