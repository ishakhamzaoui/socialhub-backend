using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Pagination;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Repositories;
 
/// <summary>See IFeedRepository's remarks and script 55's header for the full design rationale.</summary>
public sealed class FeedRepository : IFeedRepository
{
    // Known, flagged simplification — see script 55's header. Bounds how
    // many candidate rows each source query (posts, reposts) can return
    // before the final cursor-filter/sort/page step runs in memory.
    private const int FeedCandidateWindowSize = 300;
 
    private const int TrendingCandidateLimit = 300;
    private static readonly TimeSpan TrendingWindow = TimeSpan.FromDays(7);
 
    private readonly IApplicationDbContext _context;
 
    public FeedRepository(IApplicationDbContext context)
    {
        _context = context;
    }
 
    public async Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetFollowingFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default)
    {
        var followedIds = FollowedAuthorIds(requesterId);
 
        var postsQuery = VisiblePosts(requesterId).Where(p => followedIds.Contains(p.AuthorId));
        var repostsQuery = ExcludeBlockedReposters(_context.Set<PostRepost>().Where(r => followedIds.Contains(r.UserId)), requesterId);
 
        var postEntries = await FetchPostEntriesAsync(postsQuery, cancellationToken);
        var repostEntries = await FetchRepostEntriesAsync(repostsQuery, requesterId, cancellationToken);
 
        return PageInMemory(postEntries.Concat(repostEntries), cursor, pageSize);
    }
 
    public async Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetChronologicalFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default)
    {
        var postsQuery = VisiblePosts(requesterId);
        var repostsQuery = ExcludeBlockedReposters(_context.Set<PostRepost>().AsQueryable(), requesterId);
 
        var postEntries = await FetchPostEntriesAsync(postsQuery, cancellationToken);
        var repostEntries = await FetchRepostEntriesAsync(repostsQuery, requesterId, cancellationToken);
 
        return PageInMemory(postEntries.Concat(repostEntries), cursor, pageSize);
    }
 
    public async Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetPersonalizedFeedAsync(Guid requesterId, IReadOnlyList<Guid> suggestedUserIds, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default)
    {
        var followedIds = FollowedAuthorIds(requesterId);
 
        var postsQuery = VisiblePosts(requesterId)
            .Where(p => followedIds.Contains(p.AuthorId) || suggestedUserIds.Contains(p.AuthorId));
        var repostsQuery = ExcludeBlockedReposters(_context.Set<PostRepost>().Where(r => followedIds.Contains(r.UserId)), requesterId);
 
        var postEntries = await FetchPostEntriesAsync(postsQuery, cancellationToken);
        var repostEntries = await FetchRepostEntriesAsync(repostsQuery, requesterId, cancellationToken);
 
        return PageInMemory(postEntries.Concat(repostEntries), cursor, pageSize);
    }
 
    public async Task<(IReadOnlyList<FeedEntry> Entries, bool HasMore)> GetTrendingFeedAsync(Guid requesterId, FeedCursor? cursor, int pageSize, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow - TrendingWindow;
 
        var candidates = await VisiblePosts(requesterId)
            .Where(p => p.CreatedAtUtc >= since)
            .Select(p => new
            {
                p.Id,
                p.CreatedAtUtc,
                RepostCount = _context.Set<PostRepost>().Count(r => r.OriginalPostId == p.Id),
            })
            .OrderByDescending(x => x.RepostCount)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(TrendingCandidateLimit)
            .ToListAsync(cancellationToken);
 
        // Trending's SortKey is the repost count, NOT a timestamp — see
        // FeedEntry's remarks. FeedTimestampUtc is still the post's own
        // CreatedAtUtc, for display purposes only.
        var entries = candidates
            .Select(c => new FeedEntry(c.Id, null, c.CreatedAtUtc, c.RepostCount))
            .ToList();
 
        return PageInMemory(entries, cursor, pageSize);
    }
 
    /// <summary>
    /// Mirrors PostAccessPolicy's Owner/Allowed/Denied/Blocked semantics as
    /// a set-based query (see script 55's header for why the async
    /// per-item policy itself can't be reused here). Private and Unlisted
    /// posts are excluded from every feed unconditionally.
    /// </summary>
    private IQueryable<Post> VisiblePosts(Guid requesterId) =>
        _context.Set<Post>()
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => p.Visibility != PostVisibility.Private && p.Visibility != PostVisibility.Unlisted)
            .Where(p => p.AuthorId == requesterId
                || p.Visibility == PostVisibility.Public
                || (p.Visibility == PostVisibility.FollowersOnly
                    && _context.Set<Follow>().Any(f => f.FollowerId == requesterId && f.FollowingId == p.AuthorId)))
            .Where(p => !_context.Set<UserBlock>().Any(b =>
                (b.BlockerId == requesterId && b.BlockedId == p.AuthorId)
                || (b.BlockerId == p.AuthorId && b.BlockedId == requesterId)));
 
    /// <summary>A repost itself is hidden if the REPOSTER is blocked either direction from the requester — independent of whether the underlying post is visible.</summary>
    private IQueryable<PostRepost> ExcludeBlockedReposters(IQueryable<PostRepost> query, Guid requesterId) =>
        query.Where(r => !_context.Set<UserBlock>().Any(b =>
            (b.BlockerId == requesterId && b.BlockedId == r.UserId)
            || (b.BlockerId == r.UserId && b.BlockedId == requesterId)));
 
    private IQueryable<Guid> FollowedAuthorIds(Guid requesterId) =>
        _context.Set<Follow>().Where(f => f.FollowerId == requesterId).Select(f => f.FollowingId);
 
    private async Task<List<FeedEntry>> FetchPostEntriesAsync(IQueryable<Post> postsQuery, CancellationToken cancellationToken) =>
        await postsQuery
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(FeedCandidateWindowSize)
            .Select(p => new FeedEntry(p.Id, null, p.CreatedAtUtc, p.CreatedAtUtc.Ticks))
            .ToListAsync(cancellationToken);
 
    /// <summary>
    /// Two-phase: fetch bounded repost candidates first, then check which
    /// of their original posts are still visible to the requester (a
    /// repost of a post that's since gone Private/been blocked/deleted
    /// should not surface, even though the PostRepost row itself survives —
    /// same "don't trust a stale reference" caution as
    /// PostDtoFactory.BuildQuotedPreviewAsync).
    /// </summary>
    private async Task<List<FeedEntry>> FetchRepostEntriesAsync(IQueryable<PostRepost> repostsQuery, Guid requesterId, CancellationToken cancellationToken)
    {
        var candidates = await repostsQuery
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(FeedCandidateWindowSize)
            .Select(r => new { r.OriginalPostId, r.UserId, r.CreatedAtUtc })
            .ToListAsync(cancellationToken);
 
        if (candidates.Count == 0)
        {
            return new List<FeedEntry>();
        }
 
        var originalIds = candidates.Select(c => c.OriginalPostId).Distinct().ToList();
        var visibleOriginalIds = (await VisiblePosts(requesterId)
            .Where(p => originalIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();
 
        return candidates
            .Where(c => visibleOriginalIds.Contains(c.OriginalPostId))
            .Select(c => new FeedEntry(c.OriginalPostId, c.UserId, c.CreatedAtUtc, c.CreatedAtUtc.Ticks))
            .ToList();
    }
 
    /// <summary>Final cursor-filter + sort + page step, done in memory — see script 55's header for why.</summary>
    private static (IReadOnlyList<FeedEntry> Entries, bool HasMore) PageInMemory(IEnumerable<FeedEntry> entries, FeedCursor? cursor, int pageSize)
    {
        IEnumerable<FeedEntry> ordered = entries
            .OrderByDescending(e => e.SortKey)
            .ThenByDescending(e => e.PostId);
 
        if (cursor is { } c)
        {
            ordered = ordered.Where(e =>
                e.SortKey < c.SortKey
                || (e.SortKey == c.SortKey && e.PostId.CompareTo(c.TieBreakerId) < 0));
        }
 
        var page = ordered.Take(pageSize + 1).ToList();
        var hasMore = page.Count > pageSize;
 
        return (page.Take(pageSize).ToList(), hasMore);
    }
}