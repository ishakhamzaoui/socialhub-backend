using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class HashtagRepository : RepositoryBase<Hashtag, Guid>, IHashtagRepository
{
    private readonly IApplicationDbContext _context;
 
    public HashtagRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<Hashtag?> GetByNormalizedTagAsync(string normalizedTag, CancellationToken cancellationToken = default) =>
        await _context.Set<Hashtag>().FirstOrDefaultAsync(h => h.NormalizedTag == normalizedTag, cancellationToken);
}