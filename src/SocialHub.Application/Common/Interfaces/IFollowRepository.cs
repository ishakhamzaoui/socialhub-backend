using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IFollowRepository : IRepository<Follow, Guid>
{
    Task<Follow?> GetAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
 
    Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
 
    Task<int> GetFollowerCountAsync(Guid userId, CancellationToken cancellationToken = default);
 
    Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Users who follow userId. excludeUserIds (added in script 27) is
    /// applied inside the query itself, so TotalCount and pagination stay
    /// correct when the caller needs to hide blocked-relationship users —
    /// see script 27's header comment for why this can't be filtered
    /// post-hoc in the handler.
    /// </summary>
    Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowerIdsAsync(Guid userId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default);
 
    /// <summary>Users userId follows. Same excludeUserIds contract as GetFollowerIdsAsync.</summary>
    Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowingIdsAsync(Guid userId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Roadmap 5.14, mutual-followers-based only (Phase 5 scope decision —
    /// see context doc): users followed by people userId follows, excluding
    /// userId itself, anyone userId already follows, and excludeUserIds.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetSuggestedUserIdsAsync(Guid userId, int limit, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default);
 
    /// <summary>Removes any Follow row between the two users in either direction. Used by BlockUserCommand (script 28).</summary>
    Task RemoveBetweenAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default);
}