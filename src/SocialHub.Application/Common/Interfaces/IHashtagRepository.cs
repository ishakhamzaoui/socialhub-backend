using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IHashtagRepository : IRepository<Hashtag, Guid>
{
    Task<Hashtag?> GetByNormalizedTagAsync(string normalizedTag, CancellationToken cancellationToken = default);
}