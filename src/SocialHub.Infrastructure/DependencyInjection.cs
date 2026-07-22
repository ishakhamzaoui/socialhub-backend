using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Infrastructure.BackgroundJobs;
using SocialHub.Infrastructure.Cache;
using SocialHub.Infrastructure.Email;
using SocialHub.Infrastructure.Media;
using SocialHub.Infrastructure.Storage;
using StackExchange.Redis;
 
namespace SocialHub.Infrastructure;
 
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
 
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:Redis is not configured — required for Redis-backed rate limiting (spec §21).");
            }
 
            return ConnectionMultiplexer.Connect(connectionString);
        });
 
        // Phase 8 (script 54): real ICacheService, overriding Phase 1's
        // NullCacheService default — this registration runs after
        // AddApplication() in Program.cs, so it wins for constructor
        // injection (see Codebase-Navigation §3's "Null default" pattern).
        // Reuses the IConnectionMultiplexer registered immediately above.
        services.AddScoped<ICacheService, RedisCacheService>();
 
        // Phase 4: Media Infrastructure (spec §22, §15.11; roadmap 4.1-4.7).
        // All three are singletons: none hold per-request state, only the
        // configured StorageOptions (or, for the media processors, nothing
        // at all beyond the interface implementation itself).
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IImageProcessingService, SkiaImageProcessingService>();
        services.AddSingleton<IVideoProcessingService, FfmpegVideoProcessingService>();
        services.AddHostedService<MediaCleanupService>();
 
        // Phase 6 — Posts (roadmap 6.7). Needs Scoped dependencies
        // (IPostRepository/IUnitOfWork), unlike every other service
        // registered above — see ScheduledPostPublishingService's remarks.
        services.AddHostedService<ScheduledPostPublishingService>();
 
        return services;
    }
}