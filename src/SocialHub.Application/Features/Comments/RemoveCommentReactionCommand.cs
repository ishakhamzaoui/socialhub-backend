using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed record RemoveCommentReactionCommand(Guid CommentId) : ICommand, IRequireAuthorization;
