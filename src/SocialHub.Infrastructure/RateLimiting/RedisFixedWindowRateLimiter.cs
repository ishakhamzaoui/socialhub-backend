using System.Threading.RateLimiting;
using StackExchange.Redis;
 
namespace SocialHub.Infrastructure.RateLimiting;
 
/// <summary>
/// Fixed-window rate limiter with ALL counter state in Redis (an atomic
/// INCR+PEXPIRE Lua script — no in-memory state at all), so the limit is
/// enforced consistently across every API instance behind the load balancer.
/// This is why a custom limiter exists instead of System.Threading
/// .RateLimiting's built-in FixedWindowRateLimiter, which only tracks state
/// per-process — spec §21 / roadmap 3.13 require Redis-backed counters
/// specifically for multi-instance consistency.
/// </summary>
public sealed class RedisFixedWindowRateLimiter : RateLimiter
{
    private const string IncrementAndExpireScript = """
        local count = redis.call('INCR', KEYS[1])
        if count == 1 then
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
        end
        return count
        """;
 
    private readonly IConnectionMultiplexer _redis;
    private readonly string _redisKey;
    private readonly int _permitLimit;
    private readonly TimeSpan _window;
 
    public RedisFixedWindowRateLimiter(IConnectionMultiplexer redis, string partitionKey, int permitLimit, TimeSpan window)
    {
        _redis = redis;
        _redisKey = $"ratelimit:{partitionKey}";
        _permitLimit = permitLimit;
        _window = window;
    }
 
    public override TimeSpan? IdleDuration => null;
 
    protected override RateLimitLease AttemptAcquireCore(int permitCount) =>
        AcquireAsyncCore(permitCount, CancellationToken.None).AsTask().GetAwaiter().GetResult();
 
    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
 
        var result = await db.ScriptEvaluateAsync(
            IncrementAndExpireScript,
            new RedisKey[] { _redisKey },
            new RedisValue[] { (long)_window.TotalMilliseconds });
 
        var count = (long)result;
 
        if (count <= _permitLimit)
        {
            return new RedisRateLimitLease(true, null);
        }
 
        var ttl = await db.KeyTimeToLiveAsync(_redisKey);
        return new RedisRateLimitLease(false, ttl);
    }
 
    public override RateLimiterStatistics? GetStatistics() => null;
 
    protected override void Dispose(bool disposing)
    {
        // No unmanaged/in-memory state to release — all state lives in Redis.
    }
}