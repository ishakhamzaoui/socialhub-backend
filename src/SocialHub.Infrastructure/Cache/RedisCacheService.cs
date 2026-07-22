using System.Text.Json;
using SocialHub.Application.Common.Interfaces;
using StackExchange.Redis;
 
namespace SocialHub.Infrastructure.Cache;
 
/// <summary>
/// First real ICacheService implementation this codebase has had — see
/// script 54's header for why NullCacheService was the only registration
/// through the end of Phase 7. Reuses the same IConnectionMultiplexer
/// singleton already registered for RedisFixedWindowRateLimiter (one Redis
/// connection, shared by both concerns).
///
/// Values are JSON-serialized via System.Text.Json. Keys are stored exactly
/// as provided by the caller (typically an ICacheableQuery.CacheKey) — no
/// additional prefix is added here, so a caller's own key already fully
/// determines Redis namespacing.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
 
    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
 
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
 
        if (!value.HasValue)
        {
            return default;
        }
 
        try
        {
            return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (JsonException)
        {
            // A stale/incompatible cached shape (e.g. after a deploy changed
            // a DTO) should degrade to a cache miss, never throw and break
            // the request the cache was supposed to speed up.
            return default;
        }
    }
 
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serialized, expiration);
    }
 
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}