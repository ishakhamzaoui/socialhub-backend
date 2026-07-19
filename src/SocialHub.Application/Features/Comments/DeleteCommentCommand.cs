using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed record DeleteCommentCommand(Guid CommentId) : ICommand, IRequireAuthorization;