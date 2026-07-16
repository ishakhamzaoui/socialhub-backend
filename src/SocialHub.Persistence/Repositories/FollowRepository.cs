using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class FollowRepository : RepositoryBase<Follow, Guid>, IFollowRepository
{
    private readonly IApplicationDbContext _context;
 
    public FollowRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<Follow?> GetAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default) =>
        await _context.Set<Follow>().FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
 
    public async Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default) =>
        await _context.Set<Follow>().AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
 
    public async Task<int> GetFollowerCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<Follow>().CountAsync(f => f.FollowingId == userId, cancellationToken);
 
    public async Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<Follow>().CountAsync(f => f.FollowerId == userId, cancellationToken);
 
    public async Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowerIdsAsync(Guid userId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Follow>().Where(f => f.FollowingId == userId);
        if (excludeUserIds is { Count: > 0 })
        {
            query = query.Where(f => !excludeUserIds.Contains(f.FollowerId));
        }
 
        var total = await query.CountAsync(cancellationToken);
        var ids = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FollowerId)
            .ToListAsync(cancellationToken);
        return (ids, total);
    }
 
    public async Task<(IReadOnlyList<Guid> UserIds, int TotalCount)> GetFollowingIdsAsync(Guid userId, int page, int pageSize, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Follow>().Where(f => f.FollowerId == userId);
        if (excludeUserIds is { Count: > 0 })
        {
            query = query.Where(f => !excludeUserIds.Contains(f.FollowingId));
        }
 
        var total = await query.CountAsync(cancellationToken);
        var ids = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FollowingId)
            .ToListAsync(cancellationToken);
        return (ids, total);
    }
 
    public async Task<IReadOnlyList<Guid>> GetSuggestedUserIdsAsync(Guid userId, int limit, IReadOnlyCollection<Guid>? excludeUserIds = null, CancellationToken cancellationToken = default)
    {
        var followingIds = _context.Set<Follow>()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId);
 
        var query = _context.Set<Follow>()
            .Where(f => followingIds.Contains(f.FollowerId) && f.FollowingId != userId)
            .Where(f => !followingIds.Contains(f.FollowingId));
 
        if (excludeUserIds is { Count: > 0 })
        {
            query = query.Where(f => !excludeUserIds.Contains(f.FollowingId));
        }
 
        return await query
            .GroupBy(f => f.FollowingId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
 
    public async Task RemoveBetweenAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Set<Follow>()
            .Where(f =>
                (f.FollowerId == userIdA && f.FollowingId == userIdB) ||
                (f.FollowerId == userIdB && f.FollowingId == userIdA))
            .ToListAsync(cancellationToken);
 
        foreach (var row in rows)
        {
            row.MarkUnfollowed();
            _context.Set<Follow>().Remove(row);
        }
    }
}