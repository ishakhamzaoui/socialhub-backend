using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>Roadmap 7.3 (edit). Content only — mentions are set at creation and not re-editable here, same scope boundary Phase 6 drew for UpdatePostCommand (flagged there as deliberate, not silently narrowed; applies here for the same reason).</summary>
public sealed record UpdateCommentCommand(Guid CommentId, string Content) : ICommand<CommentDto>, IRequireAuthorization, ITransactionalRequest;