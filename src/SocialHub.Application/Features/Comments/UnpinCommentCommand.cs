using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed record UnpinCommentCommand(Guid CommentId) : ICommand, IRequireAuthorization, ITransactionalRequest;