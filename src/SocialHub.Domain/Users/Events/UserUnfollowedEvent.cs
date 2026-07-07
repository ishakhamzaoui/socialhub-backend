using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users.Events;
 
/// <summary>Not in spec §16's catalog (which only lists the follow, not unfollow, case) but added for symmetry — costs nothing and Phase 8/11 may want it (e.g. to retract a stale notification).</summary>
public sealed record UserUnfollowedEvent(Guid FollowerId, Guid FollowingId) : BaseEvent;