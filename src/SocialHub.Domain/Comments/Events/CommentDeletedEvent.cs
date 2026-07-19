using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Comments.Events;
 
/// <summary>
/// Matches the domain event catalog in SocialHub-Backend-Specification.md
/// §16 ("CommentDeleted"). Raised by Comment.MarkDeleted() — note this is a
/// SOFT delete (see Comment.cs's remarks), the row survives; this event
/// still fires because the comment is gone from the user's perspective.
/// </summary>
public sealed record CommentDeletedEvent(Guid CommentId, Guid PostId, Guid AuthorId) : BaseEvent;