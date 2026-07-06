using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Media.Events;
 
public sealed record MediaUploadedEvent(Guid MediaAssetId, Guid OwnerId, MediaKind Kind, MediaCategory Category) : BaseEvent;