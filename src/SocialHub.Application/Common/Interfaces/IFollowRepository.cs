using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IFollowRepository : IRepository<Follow, Guid>
{
    Task<Follow?> GetAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
 
    Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
 
    Task<int> GetFollowerCountAsync(Guid userId, CancellationToken cancellationToken = default);
 
    Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>Users who follow userId. Returns raw ids + total count; the handler resolves DTOs via IUserProfileRepository.</summary>
    Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowerIdsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>Users userId follows.</summary>
    Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowingIdsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Roadmap 5.14, mutual-followers-based only (Phase 5 scope decision —
    /// see context doc): users followed by people userId follows, excluding
    /// userId itself and anyone userId already follows. Blocked-either-
    /// direction filtering is applied by the caller (handler), same as
    /// followers/following lists, to keep block-awareness in one place
    /// (UserBlockFilter, introduced in the Application feature slice).
    /// </summary>
    Task<IReadOnlyList<Guid>> GetSuggestedUserIdsAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
 
    /// <summary>Removes any Follow row between the two users in either direction. Used by BlockUserCommandHandler.</summary>
    Task RemoveBetweenAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default);
}