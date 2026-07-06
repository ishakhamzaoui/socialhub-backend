using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Infrastructure.BackgroundJobs;
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
 
        // Phase 4: Media Infrastructure (spec §22, §15.11; roadmap 4.1-4.7).
        // All three are singletons: none hold per-request state, only the
        // configured StorageOptions (or, for the media processors, nothing
        // at all beyond the interface implementation itself).
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IImageProcessingService, SkiaImageProcessingService>();
        services.AddSingleton<IVideoProcessingService, FfmpegVideoProcessingService>();
        services.AddHostedService<MediaCleanupService>();
 
        return services;
    }
}