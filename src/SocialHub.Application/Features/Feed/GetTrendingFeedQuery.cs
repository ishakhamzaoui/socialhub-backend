using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>
/// Roadmap 8.3. Confirmed "keep it simple" design: ranked by repost count
/// within a recent window, recency as tiebreak — no comment/reaction
/// engagement signal yet (deferred to Phase 13+). See script 56's header
/// for the RequesterId field.
/// </summary>
public sealed record GetTrendingFeedQuery(Guid RequesterId, string? Cursor, int PageSize)
    : IQuery<CursorPagedFeedDto>, IRequireAuthorization, ICacheableQuery
{
    public string CacheKey => $"feed:trending:{RequesterId}:{Cursor ?? "start"}:{PageSize}";
 
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}