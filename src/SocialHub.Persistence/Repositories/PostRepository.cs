using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Posts;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class PostRepository : RepositoryBase<Post, Guid>, IPostRepository
{
    private readonly IApplicationDbContext _context;
 
    public PostRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Set<Post>()
            .Include(p => p.Media)
            .Include(p => p.Hashtags)
            .Include(p => p.Mentions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
 
    public async Task<Post?> GetByIdForAuthorAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default) =>
        await _context.Set<Post>()
            .Include(p => p.Media)
            .Include(p => p.Hashtags)
            .Include(p => p.Mentions)
            .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == authorId, cancellationToken);
 
    public async Task<Post?> GetPinnedPostAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        await _context.Set<Post>()
            .FirstOrDefaultAsync(p => p.AuthorId == authorId && p.IsPinned, cancellationToken);
 
    public async Task<(IReadOnlyList<Post> Posts, int TotalCount)> GetByAuthorAsync(Guid authorId, PostStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Post>().Where(p => p.AuthorId == authorId);
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
 
        var total = await query.CountAsync(cancellationToken);
        var posts = await query
            .Include(p => p.Media)
            .Include(p => p.Hashtags)
            .Include(p => p.Mentions)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
 
        return (posts, total);
    }
 
    public async Task<IReadOnlyList<Post>> GetDuePostsForPublishingAsync(DateTime asOfUtc, CancellationToken cancellationToken = default) =>
        await _context.Set<Post>()
            .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledForUtc != null && p.ScheduledForUtc <= asOfUtc)
            .ToListAsync(cancellationToken);
}