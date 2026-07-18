using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Infrastructure.BackgroundJobs;
 
/// <summary>
/// Publishes Scheduled posts whose ScheduledForUtc has arrived (roadmap 6.7).
/// Implemented as a plain BackgroundService per spec §3.10, same as
/// MediaCleanupService — but unlike that service, this one needs Scoped
/// dependencies (IPostRepository/IUnitOfWork), so it goes through
/// IServiceScopeFactory to create a fresh scope per pass rather than
/// constructor-injecting them directly (see this script's header for why).
/// Phase 16 formalizes and consolidates every hosted service platform-wide;
/// this one is written to need no changes when that happens (spec §23
/// already lists "scheduled post publishing" as one of the standing
/// background services).
/// </summary>
public sealed class ScheduledPostPublishingService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(1);
 
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledPostPublishingService> _logger;
 
    public ScheduledPostPublishingService(IServiceScopeFactory scopeFactory, ILogger<ScheduledPostPublishingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishDuePostsOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Scheduled post publishing pass failed; will retry on the next interval.");
            }
 
            try
            {
                await Task.Delay(RunInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }
        }
    }
 
    private async Task PublishDuePostsOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var postRepository = scope.ServiceProvider.GetRequiredService<IPostRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
 
        var duePosts = await postRepository.GetDuePostsForPublishingAsync(DateTime.UtcNow, cancellationToken);
        if (duePosts.Count == 0)
        {
            return;
        }
 
        foreach (var post in duePosts)
        {
            post.Publish();
        }
 
        await unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Published {Count} scheduled post(s).", duePosts.Count);
    }
}