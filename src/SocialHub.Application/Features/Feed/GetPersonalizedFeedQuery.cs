using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>
/// Roadmap 8.4. Confirmed design: the following feed plus posts from the
/// requester's suggested (mutual-followers) users mixed in — reuses Phase
/// 5's GetSuggestedUserIdsAsync logic rather than a new recommendation
/// engine (that's Phase 20 territory). See script 56's header for the
/// RequesterId field.
/// </summary>
public sealed record GetPersonalizedFeedQuery(Guid RequesterId, string? Cursor, int PageSize)
    : IQuery<CursorPagedFeedDto>, IRequireAuthorization, ICacheableQuery
{
    public string CacheKey => $"feed:personalized:{RequesterId}:{Cursor ?? "start"}:{PageSize}";
 
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}