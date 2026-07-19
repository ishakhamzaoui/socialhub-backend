using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Shared mapping helper, same role as Posts/PostDtoFactory — every handler/
/// query that returns a CommentDto calls this rather than duplicating the
/// shape inline. NOTE: comment.Mentions must already be loaded (via
/// GetByIdWithDetailsAsync/GetByIdForAuthorAsync/GetTopLevelForPostAsync/
/// GetRepliesAsync, or because it's the same in-memory instance just
/// created) before calling this — it does not itself force-load Mentions.
/// </summary>
public static class CommentDtoFactory
{
    public static async Task<CommentDto> CreateAsync(
        Comment comment,
        Guid requesterId,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        CancellationToken cancellationToken = default)
    {
        var replyCount = await commentRepository.GetReplyCountAsync(comment.Id, cancellationToken);
        var reactionCounts = await commentReactionRepository.GetCountsByTypeAsync(comment.Id, cancellationToken);
        var myReaction = await commentReactionRepository.GetAsync(comment.Id, requesterId, cancellationToken);
 
        return new CommentDto(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            comment.ParentCommentId,
            comment.Content,
            comment.IsPinned,
            comment.IsDeleted,
            comment.CreatedAtUtc,
            comment.UpdatedAtUtc,
            comment.Mentions.Select(m => m.MentionedUserId).ToList(),
            replyCount,
            reactionCounts,
            myReaction?.Type);
    }
}