using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Users.Follow;
 
/// <summary>Lightweight shape for followers/following/suggested-user list items — deliberately not the full UserProfileDto (no bio/counts/etc. needed for a list row).</summary>
public sealed record UserSummaryDto(Guid UserId, string DisplayName, string? AvatarUrl, bool IsVerified)
{
    public static UserSummaryDto From(UserProfile profile) => new(
        profile.UserId,
        profile.DisplayName,
        profile.AvatarMediaId is not null ? $"/api/v1/users/{profile.UserId}/avatar" : null,
        profile.IsVerified);
}
 
public sealed record PagedUserListDto(IReadOnlyList<UserSummaryDto> Users, int TotalCount, int Page, int PageSize);