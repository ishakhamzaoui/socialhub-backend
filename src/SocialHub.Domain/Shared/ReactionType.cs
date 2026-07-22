namespace SocialHub.Domain.Shared;
 
/// <summary>
/// Roadmap 7.5 (Likes) / 7.6 (Reactions), extended by roadmap 8's PostReaction
/// (script 51). Confirmed-by-default decision (Phase 7 kickoff, item 6 in
/// script 42's header): ONE unified mechanism. Like is simply ReactionType.Like,
/// one value among several — there is no separate boolean "IsLiked" concept
/// anywhere in this codebase. A simple fixed enum for now, extensibility
/// deliberately deferred (mirrors Phase 6's tight hashtag/mention scope
/// restraint) — revisit only if a later phase genuinely needs custom/
/// community-defined reaction types.
///
/// MOVED from Domain/Comments to Domain/Shared in script 51 (Phase 8) because
/// a second aggregate — PostReaction — now uses it too; this is the same
/// "cross-feature, no single owning aggregate" home Hashtag already lives in.
/// The enum's values and underlying semantics are unchanged by the move.
/// </summary>
public enum ReactionType
{
    Like = 0,
    Love = 1,
    Laugh = 2,
    Sad = 3,
    Angry = 4
}