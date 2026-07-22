using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>Roadmap 8.2 — the platform-wide public timeline, not limited to who the requester follows. See script 56's header for the RequesterId field.</summary>
public sealed record GetChronologicalFeedQuery(Guid RequesterId, string? Cursor, int PageSize)
    : IQuery<CursorPagedFeedDto>, IRequireAuthorization, ICacheableQuery
{
    public string CacheKey => $"feed:chronological:{RequesterId}:{Cursor ?? "start"}:{PageSize}";
 
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}