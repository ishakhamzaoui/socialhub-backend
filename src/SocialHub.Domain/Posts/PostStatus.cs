namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.6/6.7. Confirmed decision: drafts and scheduled posts are one
/// lifecycle on Post itself (explicit status + timestamps), not two
/// separate concepts. Valid transitions (enforced in Post's methods, not
/// here): Draft -> Scheduled -> Published -> Archived, Draft -> Published
/// directly, Published -> Archived. Archived is currently terminal (no
/// Unarchive roadmap step exists yet; Post.Restore() is provided for
/// completeness but nothing in Phase 6 calls it from the API).
/// </summary>
public enum PostStatus
{
    Draft = 0,
    Scheduled = 1,
    Published = 2,
    Archived = 3
}