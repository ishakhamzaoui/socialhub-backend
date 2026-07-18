using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.8.</summary>
public sealed record ArchivePostCommand(Guid PostId) : ICommand, IRequireAuthorization, ITransactionalRequest;