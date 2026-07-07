using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IUserBlockRepository : IRepository<UserBlock, Guid>
{
    Task<UserBlock?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default);
 
    Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default);
 
    Task<bool> IsBlockedEitherDirectionAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default);
 
    /// <summary>Every userId blockerId has blocked. Used to filter follower/following/suggested-user lists.</summary>
    Task<IReadOnlyList<Guid>> GetBlockedUserIdsAsync(Guid blockerId, CancellationToken cancellationToken = default);
 
    /// <summary>Every userId that has blocked userId. Used the same way, from the other direction.</summary>
    Task<IReadOnlyList<Guid>> GetBlockedByUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}