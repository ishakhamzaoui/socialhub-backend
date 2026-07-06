using SocialHub.Domain.Media;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IMediaAssetRepository : IRepository<MediaAsset, Guid>
{
    Task<MediaAsset?> GetByIdForOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
 
    Task<IReadOnlyList<MediaAsset>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
}