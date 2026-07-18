using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Common.Policies;
 
public enum PostAccessResult
{
    /// <summary>Requester is the post's author — always allowed, regardless of Status/Visibility.</summary>
    Owner,
    Allowed,
    Denied,
 
    /// <summary>Blocked either direction. Callers should return NotFound rather than Forbidden, same non-leaking convention as ProfileAccessResult.Blocked.</summary>
    Blocked
}
 
/// <summary>
/// "Can this requester see this post" (roadmap 6.4's visibility rules),
/// mirroring ProfileAccessPolicy's shape exactly. Shared by GetPostQuery
/// (this script) and any later feature that needs to check post visibility
/// (Phase 8's feed will very likely reuse this per-post, once it exists).
/// </summary>
public static class PostAccessPolicy
{
    public static async Task<PostAccessResult> EvaluateAsync(
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        Post post,
        Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        if (requesterId == post.AuthorId)
        {
            return PostAccessResult.Owner;
        }
 
        if (await userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, cancellationToken))
        {
            return PostAccessResult.Blocked;
        }
 
        // A non-owner never sees a Draft/Scheduled/Archived post, no matter
        // what Visibility says — Visibility only governs a Published post.
        if (post.Status != PostStatus.Published)
        {
            return PostAccessResult.Denied;
        }
 
        var canView = post.Visibility switch
        {
            PostVisibility.Public => true,
            PostVisibility.Unlisted => true,
            PostVisibility.FollowersOnly => await followRepository.ExistsAsync(requesterId, post.AuthorId, cancellationToken),
            PostVisibility.Private => false,
            _ => false
        };
 
        return canView ? PostAccessResult.Allowed : PostAccessResult.Denied;
    }
}