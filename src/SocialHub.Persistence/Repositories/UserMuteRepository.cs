using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class UserMuteRepository : RepositoryBase<UserMute, Guid>, IUserMuteRepository
{
    private readonly IApplicationDbContext _context;
 
    public UserMuteRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<UserMute?> GetAsync(Guid muterId, Guid mutedId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserMute>().FirstOrDefaultAsync(m => m.MuterId == muterId && m.MutedId == mutedId, cancellationToken);
 
    public async Task<bool> IsMutedAsync(Guid muterId, Guid mutedId, CancellationToken cancellationToken = default) =>
        await _context.Set<UserMute>().AnyAsync(m => m.MuterId == muterId && m.MutedId == mutedId, cancellationToken);
}