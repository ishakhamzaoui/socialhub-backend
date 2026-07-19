using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed record CommentDto(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    Guid? ParentCommentId,
    string? Content,
    bool IsPinned,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<Guid> MentionedUserIds,
    int ReplyCount,
    IReadOnlyDictionary<ReactionType, int> ReactionCounts,
    ReactionType? MyReaction);