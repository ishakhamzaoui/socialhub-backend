using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Media.Events;
 
public sealed record MediaDeletedEvent(Guid MediaAssetId, Guid OwnerId) : BaseEvent;