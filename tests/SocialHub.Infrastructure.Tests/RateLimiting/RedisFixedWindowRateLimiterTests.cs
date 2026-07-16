using FluentAssertions;
using SocialHub.Infrastructure.RateLimiting;
using StackExchange.Redis;
using Xunit;

namespace SocialHub.Infrastructure.Tests.RateLimiting;

/// <summary>
/// Uses the real dev Redis instance, never a mock — the entire point of this
/// limiter is atomic server-side counting, which a mock can't meaningfully
/// verify. Override via SOCIALHUB_TEST_REDIS_CONNECTION if your setup
/// differs from the localhost:6379 Phase 0 installed.
/// </summary>
public class RedisFixedWindowRateLimiterTests
{
    private static IConnectionMultiplexer CreateConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("SOCIALHUB_TEST_REDIS_CONNECTION") ?? "localhost:6379";
        return ConnectionMultiplexer.Connect(connectionString);
    }

    [Fact]
    public async Task AcquireAsync_Should_AllowRequestsWithinLimit_Then_RejectOverLimit()
    {
        using var redis = CreateConnection();
        var partitionKey = $"test:{Guid.NewGuid()}";
        using var limiter = new RedisFixedWindowRateLimiter(redis, partitionKey, permitLimit: 3, window: TimeSpan.FromSeconds(5));

        var results = new List<bool>();
        for (var i = 0; i < 4; i++)
        {
            var lease = await limiter.AcquireAsync();
            results.Add(lease.IsAcquired);
        }

        results.Should().Equal(true, true, true, false);

        await redis.GetDatabase().KeyDeleteAsync($"ratelimit:{partitionKey}");
    }

    // [Fact]
    // public async Task AcquireAsync_Should_AllowAgain_After_WindowExpires()
    // {
    //     using var redis = CreateConnection();
    //     var partitionKey = $"test:{Guid.NewGuid()}";
    //     using var limiter = new RedisFixedWindowRateLimiter(redis, partitionKey, permitLimit: 1, window: TimeSpan.FromMilliseconds(500));

    //     var first = await limiter.AcquireAsync();
    //     var second = await limiter.AcquireAsync();
    //     await Task.Delay(3000);
    //     var third = await limiter.AcquireAsync();

    //     first.IsAcquired.Should().BeTrue();
    //     second.IsAcquired.Should().BeFalse();
    //     third.IsAcquired.Should().BeTrue();

    //     await redis.GetDatabase().KeyDeleteAsync($"ratelimit:{partitionKey}");
    // }
}