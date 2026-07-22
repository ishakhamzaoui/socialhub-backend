using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class CommentReactionRepository : RepositoryBase<CommentReaction, Guid>, ICommentReactionRepository
{
    private readonly IApplicationDbContext _context;
 
    public CommentReactionRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<CommentReaction?> GetAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<CommentReaction>()
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId, cancellationToken);
 
    public async Task<IReadOnlyDictionary<ReactionType, int>> GetCountsByTypeAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Set<CommentReaction>()
            .Where(r => r.CommentId == commentId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
 
        return grouped.ToDictionary(g => g.Type, g => g.Count);
    }
}