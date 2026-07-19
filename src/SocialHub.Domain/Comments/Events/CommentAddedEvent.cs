using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Comments.Events;
 
/// <summary>
/// Matches the domain event catalog in SocialHub-Backend-Specification.md
/// §16 ("CommentAdded"). Raised whenever a Comment row is created —
/// top-level or a nested reply alike. Nothing consumes this yet in Phase
/// 7; wired up when Phase 11's notification handlers exist (mention/
/// comment notifications).
/// </summary>
public sealed record CommentAddedEvent(Guid CommentId, Guid PostId, Guid AuthorId, Guid? ParentCommentId) : BaseEvent;