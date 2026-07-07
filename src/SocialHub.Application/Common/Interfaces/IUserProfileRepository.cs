using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IUserProfileRepository : IRepository<UserProfile, Guid>
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>Batch lookup for building follower/following/suggested-user DTO lists without N+1 queries.</summary>
    Task<IReadOnlyList<UserProfile>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
}