using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>
/// AvatarUrl/CoverUrl point at the dedicated, visibility-aware endpoints
/// (GET /api/v1/users/{id}/avatar|cover, added in script 26) rather than
/// Phase 4's owner-only /api/v1/media/{id}/download — the URL is stable
/// regardless of which MediaAsset currently backs it, which also lets a
/// replaced avatar's URL keep working without the client needing to know
/// the new MediaAsset id.
/// </summary>
public sealed record UserProfileDto(
    Guid UserId,
    string DisplayName,
    string? Bio,
    string? Location,
    string? Website,
    string? AvatarUrl,
    string? CoverUrl,
    ProfileVisibility Visibility,
    ThemePreference Theme,
    string Language,
    bool IsVerified,
    int FollowerCount,
    int FollowingCount,
    bool IsFollowedByRequester,
    bool IsOwnProfile,
    DateTime CreatedAtUtc)
{
    public static UserProfileDto From(
        UserProfile profile,
        int followerCount,
        int followingCount,
        bool isFollowedByRequester,
        bool isOwnProfile) => new(
        profile.UserId,
        profile.DisplayName,
        profile.Bio,
        profile.Location,
        profile.Website,
        profile.AvatarMediaId is not null ? $"/api/v1/users/{profile.UserId}/avatar" : null,
        profile.CoverMediaId is not null ? $"/api/v1/users/{profile.UserId}/cover" : null,
        profile.Visibility,
        profile.Theme,
        profile.Language,
        profile.IsVerified,
        followerCount,
        followingCount,
        isFollowedByRequester,
        isOwnProfile,
        profile.CreatedAtUtc);
}