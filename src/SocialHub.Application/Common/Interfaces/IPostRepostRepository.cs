using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>Same shape as IFollowRepository — PostRepost relates two independent things (a user, someone else's post), so it gets its own small repository rather than living inside IPostRepository.</summary>
public interface IPostRepostRepository : IRepository<PostRepost, Guid>
{
    Task<PostRepost?> GetAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default);
 
    Task<bool> ExistsAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default);
 
    Task<int> GetRepostCountAsync(Guid originalPostId, CancellationToken cancellationToken = default);
 
    /// <summary>"Undo repost." No-ops if the row doesn't exist.</summary>
    Task RemoveAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default);
}