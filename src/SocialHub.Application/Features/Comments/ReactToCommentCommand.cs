using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Roadmap 7.5 (Likes) / 7.6 (Reactions). Flagged assumption (Phase 7
/// kickoff, item 6): one unified mechanism — Like is just ReactionType.Like.
/// Reacting again with a different type changes the existing reaction
/// rather than adding a second one (see CommentReaction's remarks).
/// </summary>
public sealed record ReactToCommentCommand(Guid CommentId, ReactionType Type) : ICommand<CommentDto>, IRequireAuthorization, ITransactionalRequest;