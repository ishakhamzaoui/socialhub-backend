using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Comments;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class CommentRepository : RepositoryBase<Comment, Guid>, ICommentRepository
{
    private readonly IApplicationDbContext _context;
 
    public CommentRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<Comment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>()
            .Include(c => c.Mentions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
 
    public async Task<Comment?> GetByIdForAuthorAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>()
            .Include(c => c.Mentions)
            .FirstOrDefaultAsync(c => c.Id == id && c.AuthorId == authorId, cancellationToken);
 
    public async Task<(IReadOnlyList<Comment> Comments, int TotalCount)> GetTopLevelForPostAsync(Guid postId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeAuthorIds = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Comment>().Where(c => c.PostId == postId && c.ParentCommentId == null);
 
        if (excludeAuthorIds is { Count: > 0 })
        {
            query = query.Where(c => !excludeAuthorIds.Contains(c.AuthorId));
        }
 
        var total = await query.CountAsync(cancellationToken);
        var comments = await query
            .Include(c => c.Mentions)
            .OrderByDescending(c => c.IsPinned)
            .ThenBy(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
 
        return (comments, total);
    }
 
    public async Task<(IReadOnlyList<Comment> Replies, int TotalCount)> GetRepliesAsync(Guid parentCommentId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeAuthorIds = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Comment>().Where(c => c.ParentCommentId == parentCommentId);
 
        if (excludeAuthorIds is { Count: > 0 })
        {
            query = query.Where(c => !excludeAuthorIds.Contains(c.AuthorId));
        }
 
        var total = await query.CountAsync(cancellationToken);
        var replies = await query
            .Include(c => c.Mentions)
            .OrderBy(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
 
        return (replies, total);
    }
 
    public async Task<int> GetReplyCountAsync(Guid parentCommentId, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>().CountAsync(c => c.ParentCommentId == parentCommentId, cancellationToken);
 
    public async Task<Comment?> GetPinnedCommentForPostAsync(Guid postId, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>().FirstOrDefaultAsync(c => c.PostId == postId && c.IsPinned, cancellationToken);
}