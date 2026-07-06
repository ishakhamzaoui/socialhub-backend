using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Infrastructure.Storage;
 
namespace SocialHub.Infrastructure.BackgroundJobs;
 
/// <summary>
/// Sweeps the "temp" upload staging area (spec §22) of files left behind by
/// interrupted or abandoned uploads (roadmap 4.7). Anything under temp/ past
/// the configured TTL is, by construction of the upload flow (see
/// IFileStorageService's remarks), guaranteed to be safe to delete — nothing
/// legitimate stays there past the single request that promotes it.
///
/// Implemented as a plain BackgroundService per spec §3.10 (native
/// IHostedService, no third-party job scheduler). Phase 16 formalizes and
/// consolidates every hosted service platform-wide; this one is written to
/// need no changes when that happens (spec §23 already lists this as one of
/// the standing background services).
/// </summary>
public sealed class MediaCleanupService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(1);
 
    private readonly IFileStorageService _fileStorageService;
    private readonly StorageOptions _options;
    private readonly ILogger<MediaCleanupService> _logger;
 
    public MediaCleanupService(IFileStorageService fileStorageService, IOptions<StorageOptions> options, ILogger<MediaCleanupService> logger)
    {
        _fileStorageService = fileStorageService;
        _options = options.Value;
        _logger = logger;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Media cleanup pass failed; will retry on the next interval.");
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
 
    private async Task CleanupOnceAsync(CancellationToken cancellationToken)
    {
        var cutoffUtc = DateTime.UtcNow.AddHours(-_options.TempFileTtlHours);
        var stalePaths = await _fileStorageService.ListTempFilesOlderThanAsync(cutoffUtc, cancellationToken);
 
        foreach (var relativePath in stalePaths)
        {
            await _fileStorageService.DeleteAsync(relativePath, cancellationToken);
            _logger.LogInformation("Deleted stale temp upload: {RelativePath}", relativePath);
        }
 
        if (stalePaths.Count > 0)
        {
            _logger.LogInformation("Media cleanup removed {Count} stale temp file(s).", stalePaths.Count);
        }
    }
}