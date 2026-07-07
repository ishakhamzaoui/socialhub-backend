using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class UserProfileRepository : RepositoryBase<UserProfile, Guid>, IUserProfileRepository
{
    private readonly IApplicationDbContext _context;
 
    public UserProfileRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
 
    public async Task<IReadOnlyList<UserProfile>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default) =>
        await _context.Set<UserProfile>().Where(p => userIds.Contains(p.UserId)).ToListAsync(cancellationToken);
}