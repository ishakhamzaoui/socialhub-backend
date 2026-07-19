using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface ICommentReactionRepository : IRepository<CommentReaction, Guid>
{
    /// <summary>The requester's existing reaction on a comment, if any — looked up first so ReactToCommentCommandHandler can call ChangeType on it instead of adding a second row (see CommentReaction's own remarks: one reaction per user per comment).</summary>
    Task<CommentReaction?> GetAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>Aggregated reaction counts by type for a comment, computed in the database rather than by loading every row — used to build CommentDto's reaction summary.</summary>
    Task<IReadOnlyDictionary<ReactionType, int>> GetCountsByTypeAsync(Guid commentId, CancellationToken cancellationToken = default);
}