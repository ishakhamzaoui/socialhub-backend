namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.4. Deliberately separate from Domain.Users.ProfileVisibility
/// (confirmed decision, Phase 6 kickoff) — post visibility and profile
/// visibility are different axes even though three of the four values look
/// similar, and Unlisted has no profile-visibility analogue at all.
/// </summary>
public enum PostVisibility
{
    Public = 0,
    FollowersOnly = 1,
    Private = 2,
 
    /// <summary>Not shown in lists/search/feeds; viewable only via direct link/ID. No profile-visibility equivalent.</summary>
    Unlisted = 3
}