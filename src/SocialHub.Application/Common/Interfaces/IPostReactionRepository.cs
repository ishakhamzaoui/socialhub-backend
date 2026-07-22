using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>Mirrors ICommentReactionRepository exactly — see that interface's remarks.</summary>
public interface IPostReactionRepository : IRepository<PostReaction, Guid>
{
    /// <summary>The requester's existing reaction on a post, if any — looked up first so ReactToPostCommandHandler can call ChangeType on it instead of adding a second row.</summary>
    Task<PostReaction?> GetAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>Aggregated reaction counts by type for a post, computed in the database rather than by loading every row — used to build PostDto's reaction summary.</summary>
    Task<IReadOnlyDictionary<ReactionType, int>> GetCountsByTypeAsync(Guid postId, CancellationToken cancellationToken = default);
}