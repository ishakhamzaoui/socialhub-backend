using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>Roadmap 7.4. Flagged assumption (not explicitly asked): only the POST's author may pin a comment on their own post — see PinCommentCommandHandler's remarks.</summary>
public sealed record PinCommentCommand(Guid CommentId) : ICommand, IRequireAuthorization, ITransactionalRequest;