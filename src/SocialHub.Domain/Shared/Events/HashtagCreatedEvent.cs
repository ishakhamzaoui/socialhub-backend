using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Shared.Events;
 
public sealed record HashtagCreatedEvent(Guid HashtagId, string Tag) : BaseEvent;