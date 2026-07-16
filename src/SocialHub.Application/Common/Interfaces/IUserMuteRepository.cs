using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IUserMuteRepository : IRepository<UserMute, Guid>
{
    Task<UserMute?> GetAsync(Guid muterId, Guid mutedId, CancellationToken cancellationToken = default);
 
    Task<bool> IsMutedAsync(Guid muterId, Guid mutedId, CancellationToken cancellationToken = default);
 
    /// <summary>Added in script 28 for GetMutedUsersQuery — every userId muterId has muted.</summary>
    Task<IReadOnlyList<Guid>> GetMutedUserIdsAsync(Guid muterId, CancellationToken cancellationToken = default);
}