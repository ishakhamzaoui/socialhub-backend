namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.10 (Quote) / 6.11 (Repost). Repost is NOT a value here —
/// confirmed decision: a repost has no content of its own and is modeled as
/// the separate PostRepost aggregate, not a Post row. Only content-bearing
/// posts are Post rows.
/// </summary>
public enum PostType
{
    Original = 0,
 
    /// <summary>Has its own Content plus OriginalPostId pointing at the quoted post.</summary>
    Quote = 1
}