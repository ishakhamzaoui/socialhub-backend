using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SocialHub.Infrastructure.RateLimiting;
using StackExchange.Redis;
 
namespace SocialHub.API.Extensions;
 
public static class RateLimitingExtensions
{
    public const string LoginPolicy = "auth-login";
    public const string RegisterPolicy = "auth-register";
    public const string PasswordResetPolicy = "auth-password-reset";
 
    /// <summary>
    /// Baseline rate limiting on authentication endpoints (roadmap 3.13),
    /// backed by Redis so the limit holds across every API instance. This is
    /// introduced here — not deferred to Phase 17 — because these endpoints
    /// are brute-force targets from the moment they exist.
    /// </summary>
    public static IServiceCollection AddSocialHubRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
 
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }
 
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests.",
                    Detail = "You have made too many requests. Please try again later.",
                    Type = "https://httpstatuses.io/429"
                };
 
                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
 
            AddIpPolicy(options, configuration, LoginPolicy, "RateLimiting:Login", defaultPermitLimit: 5, defaultWindowSeconds: 60);
            AddIpPolicy(options, configuration, RegisterPolicy, "RateLimiting:Register", defaultPermitLimit: 5, defaultWindowSeconds: 300);
            AddIpPolicy(options, configuration, PasswordResetPolicy, "RateLimiting:PasswordReset", defaultPermitLimit: 5, defaultWindowSeconds: 300);
        });
 
        return services;
    }
 
    private static void AddIpPolicy(
        RateLimiterOptions options,
        IConfiguration configuration,
        string policyName,
        string configSection,
        int defaultPermitLimit,
        int defaultWindowSeconds)
    {
        var permitLimit = configuration.GetValue<int?>($"{configSection}:PermitLimit") ?? defaultPermitLimit;
        var windowSeconds = configuration.GetValue<int?>($"{configSection}:WindowSeconds") ?? defaultWindowSeconds;
 
        options.AddPolicy(policyName, httpContext =>
        {
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var partitionKey = $"{policyName}:{clientIp}";
 
            var redis = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
 
            return RateLimitPartition.Get(partitionKey, _ =>
                new RedisFixedWindowRateLimiter(redis, partitionKey, permitLimit, TimeSpan.FromSeconds(windowSeconds)));
        });
    }
}