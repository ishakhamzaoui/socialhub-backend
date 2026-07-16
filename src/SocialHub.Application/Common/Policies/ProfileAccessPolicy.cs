using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Policies;
 
public enum ProfileAccessResult
{
    /// <summary>Requester is viewing their own profile — always allowed.</summary>
    Owner,
    Allowed,
    Denied,
 
    /// <summary>Blocked either direction. Callers should return NotFound rather than Forbidden for this case, so a block's existence isn't leaked to the blocked/blocking party.</summary>
    Blocked
}
 
/// <summary>
/// Central home for "can this requester see this profile/avatar/cover"
/// (roadmap 5.4's privacy settings). This is the concrete resolution of the
/// Phase 4 follow-up flagged in SocialHub-Context-Phases-4.md: rather than
/// loosening GetMediaQueryHandler's owner-only rule, visibility is resolved
/// here, in the Application layer, BEFORE any caller touches Media — the
/// avatar/cover controllers (script 26) and GetUserProfileQuery both call
/// this same policy so the rule only has one home.
/// </summary>
public static class ProfileAccessPolicy
{
    public static async Task<ProfileAccessResult> EvaluateAsync(
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        UserProfile profile,
        Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        if (requesterId == profile.UserId)
        {
            return ProfileAccessResult.Owner;
        }
 
        if (await userBlockRepository.IsBlockedEitherDirectionAsync(profile.UserId, requesterId, cancellationToken))
        {
            return ProfileAccessResult.Blocked;
        }
 
        var canView = profile.Visibility switch
        {
            ProfileVisibility.Public => true,
            ProfileVisibility.FollowersOnly => await followRepository.ExistsAsync(requesterId, profile.UserId, cancellationToken),
            ProfileVisibility.Private => false,
            _ => false
        };
 
        return canView ? ProfileAccessResult.Allowed : ProfileAccessResult.Denied;
    }
}