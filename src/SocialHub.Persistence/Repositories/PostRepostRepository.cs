using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Posts;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class PostRepostRepository : RepositoryBase<PostRepost, Guid>, IPostRepostRepository
{
    private readonly IApplicationDbContext _context;
 
    public PostRepostRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<PostRepost?> GetAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default) =>
        await _context.Set<PostRepost>().FirstOrDefaultAsync(r => r.UserId == userId && r.OriginalPostId == originalPostId, cancellationToken);
 
    public async Task<bool> ExistsAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default) =>
        await _context.Set<PostRepost>().AnyAsync(r => r.UserId == userId && r.OriginalPostId == originalPostId, cancellationToken);
 
    public async Task<int> GetRepostCountAsync(Guid originalPostId, CancellationToken cancellationToken = default) =>
        await _context.Set<PostRepost>().CountAsync(r => r.OriginalPostId == originalPostId, cancellationToken);
 
    public async Task RemoveAsync(Guid userId, Guid originalPostId, CancellationToken cancellationToken = default)
    {
        var row = await GetAsync(userId, originalPostId, cancellationToken);
        if (row is not null)
        {
            _context.Set<PostRepost>().Remove(row);
        }
    }
}