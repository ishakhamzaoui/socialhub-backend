namespace SocialHub.Domain.Users;
 
/// <summary>
/// Roadmap 5.4 (Privacy settings). Deliberately a single visibility level
/// covering the profile as a whole (bio/location/website) AND the avatar/
/// cover images, rather than a separate enum per field — the spec doesn't
/// call for granular per-field privacy, and Phase 6's post visibility rules
/// will introduce their own (separate) visibility scheme for posts. If a
/// future phase needs per-field granularity here, extend UserProfile with
/// additional ProfileVisibility-typed properties rather than replacing this
/// one — don't overload this single enum's meaning.
/// </summary>
public enum ProfileVisibility
{
    Public = 0,
    FollowersOnly = 1,
    Private = 2
}