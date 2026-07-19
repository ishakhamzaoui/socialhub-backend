namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.5 (Likes) / 7.6 (Reactions). Confirmed-by-default decision
/// (Phase 7 kickoff, item 6 in script 42's header): ONE unified mechanism.
/// Like is simply ReactionType.Like, one value among several — there is no
/// separate boolean "IsLiked" concept anywhere in this codebase. A simple
/// fixed enum for now, extensibility deliberately deferred (mirrors Phase
/// 6's tight hashtag/mention scope restraint) — revisit only if a later
/// phase genuinely needs custom/community-defined reaction types.
/// </summary>
public enum ReactionType
{
    Like = 0,
    Love = 1,
    Laugh = 2,
    Sad = 3,
    Angry = 4
}