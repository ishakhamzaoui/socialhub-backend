using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts.Events;
 
/// <summary>Matches the domain event catalog in SocialHub-Backend-Specification.md §16 ("PostDeleted").</summary>
public sealed record PostDeletedEvent(Guid PostId, Guid AuthorId) : BaseEvent;