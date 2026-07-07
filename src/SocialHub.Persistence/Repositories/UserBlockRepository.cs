using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class UserBlockRepository : RepositoryBase<UserBlock, Guid>, IUserBlockRepository
{
    private readonly IApplicationDbContext _context;
 
    public UserBlockRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<UserBlock?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserBlock>().FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId, cancellationToken);
 
    public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserBlock>().AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId, cancellationToken);
 
    public async Task<bool> IsBlockedEitherDirectionAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default) =>
        await _context.Set<UserBlock>().AnyAsync(b =>
            (b.BlockerId == userIdA && b.BlockedId == userIdB) ||
            (b.BlockerId == userIdB && b.BlockedId == userIdA),
            cancellationToken);
 
    public async Task<IReadOnlyList<Guid>> GetBlockedUserIdsAsync(Guid blockerId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserBlock>().Where(b => b.BlockerId == blockerId).Select(b => b.BlockedId).ToListAsync(cancellationToken);
 
    public async Task<IReadOnlyList<Guid>> GetBlockedByUserIdsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserBlock>().Where(b => b.BlockedId == userId).Select(b => b.BlockerId).ToListAsync(cancellationToken);
}