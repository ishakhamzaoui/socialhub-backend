using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts.Events;
 
/// <summary>
/// Matches the domain event catalog in SocialHub-Backend-Specification.md
/// §16 ("PostCreated"). Raised whenever a Post row comes into existence —
/// Draft, Scheduled, or Published alike (the event represents the row
/// existing, not "went live"; nothing consumes this yet in Phase 6, wired
/// up when Phase 11's notification handlers exist).
/// </summary>
public sealed record PostCreatedEvent(Guid PostId, Guid AuthorId) : BaseEvent;