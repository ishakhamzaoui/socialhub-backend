using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Application.Common.Defaults;
 
/// <summary>
/// Default ICacheService until Infrastructure registers a real Redis-backed
/// implementation. Always misses; never actually caches anything.
/// </summary>
public sealed class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T?>(default);
 
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) => Task.CompletedTask;
 
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
}