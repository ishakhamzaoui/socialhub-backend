using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>
/// Roadmap 8.1. See script 56's header for why RequesterId is an explicit
/// field here, unlike every other query/command in this codebase.
/// </summary>
public sealed record GetFollowingFeedQuery(Guid RequesterId, string? Cursor, int PageSize)
    : IQuery<CursorPagedFeedDto>, IRequireAuthorization, ICacheableQuery
{
    public string CacheKey => $"feed:following:{RequesterId}:{Cursor ?? "start"}:{PageSize}";
 
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}