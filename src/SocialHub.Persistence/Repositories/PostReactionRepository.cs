using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class PostReactionRepository : RepositoryBase<PostReaction, Guid>, IPostReactionRepository
{
    private readonly IApplicationDbContext _context;
 
    public PostReactionRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<PostReaction?> GetAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<PostReaction>()
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId, cancellationToken);
 
    public async Task<IReadOnlyDictionary<ReactionType, int>> GetCountsByTypeAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Set<PostReaction>()
            .Where(r => r.PostId == postId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
 
        return grouped.ToDictionary(g => g.Type, g => g.Count);
    }
}