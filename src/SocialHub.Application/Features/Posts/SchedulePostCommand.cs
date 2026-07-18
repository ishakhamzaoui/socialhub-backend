using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.7.</summary>
public sealed record SchedulePostCommand(Guid PostId, DateTime ScheduledForUtc) : ICommand<PostDto>, IRequireAuthorization, ITransactionalRequest;