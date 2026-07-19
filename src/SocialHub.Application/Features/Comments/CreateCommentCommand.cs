using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Roadmap 7.1 (top-level comments, ParentCommentId null) / 7.2 (nested
/// replies, ParentCommentId set) / 7.7 (mentions). MentionedUserIds is an
/// explicit client-supplied list, same confirmed decision as Phase 6's
/// Posts — no @-parsing, no username/handle concept exists in this
/// codebase to resolve one against.
/// </summary>
public sealed record CreateCommentCommand(
    Guid PostId,
    Guid? ParentCommentId,
    string Content,
    IReadOnlyList<Guid>? MentionedUserIds) : ICommand<CommentDto>, IRequireAuthorization, ITransactionalRequest;