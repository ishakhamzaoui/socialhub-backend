using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>
/// Mirrors ReactToCommentCommand exactly (Phase 7) — one unified reaction
/// mechanism, Like is just ReactionType.Like. Reacting again with a
/// different type changes the existing PostReaction row rather than adding
/// a second one (see PostReaction.ChangeType).
/// </summary>
public sealed record ReactToPostCommand(Guid PostId, ReactionType Type) : ICommand<PostDto>, IRequireAuthorization, ITransactionalRequest;