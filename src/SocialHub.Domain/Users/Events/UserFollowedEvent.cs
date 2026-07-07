using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users.Events;
 
/// <summary>Matches the domain event catalog in SocialHub-Backend-Specification.md §16 ("UserFollowed"). Consumed by Phase 11's follow notifications.</summary>
public sealed record UserFollowedEvent(Guid FollowerId, Guid FollowingId) : BaseEvent;