namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.8. A small fixed catalog for why a comment was reported.
/// Deliberately not shared with any future Post/User/Community report
/// concept yet — Phase 14 (Moderation) owns designing the general
/// cross-entity Reports system; this enum exists only to give
/// CommentReport a reason without free-text-only input.
/// </summary>
public enum CommentReportReason
{
    Spam = 0,
    Harassment = 1,
    HateSpeech = 2,
    Misinformation = 3,
    Other = 4
}