using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed record UndoRepostCommand(Guid OriginalPostId) : ICommand, IRequireAuthorization, ITransactionalRequest;