using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Media;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class MediaAssetRepository : RepositoryBase<MediaAsset, Guid>, IMediaAssetRepository
{
    private readonly IApplicationDbContext _context;
 
    public MediaAssetRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<MediaAsset?> GetByIdForOwnerAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default) =>
        await _context.Set<MediaAsset>().FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == ownerId, cancellationToken);
 
    public async Task<IReadOnlyList<MediaAsset>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await _context.Set<MediaAsset>()
            .Where(m => m.OwnerId == ownerId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}