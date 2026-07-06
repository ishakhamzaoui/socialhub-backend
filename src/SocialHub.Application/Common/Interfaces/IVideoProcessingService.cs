namespace SocialHub.Application.Common.Interfaces;
 
public sealed record VideoMetadata(double DurationSeconds, int Width, int Height);
 
/// <summary>
/// Video metadata extraction (roadmap 4.5) and thumbnail-frame generation
/// (roadmap 4.6). Implemented by
/// SocialHub.Infrastructure.Media.FfmpegVideoProcessingService, which shells
/// out to the `ffprobe`/`ffmpeg` binaries already installed on the host
/// (apt package `ffmpeg`) via System.Diagnostics.Process rather than taking
/// a NuGet dependency on FFmpeg itself — this avoids any question about
/// FFmpeg's own build-configuration-dependent licensing (LGPL/GPL), since
/// nothing is linked into or redistributed with SocialHub; it's invoked the
/// same way the deployment scripts already invoke `pg_dump`/`rsync` (spec
/// §31).
/// </summary>
public interface IVideoProcessingService
{
    Task<VideoMetadata> ExtractMetadataAsync(string absoluteSourcePath, CancellationToken cancellationToken = default);
 
    /// <summary>Extracts a single frame (roughly 1s in, or the first frame for very short clips) as a JPEG thumbnail.</summary>
    Task GenerateThumbnailAsync(string absoluteSourcePath, string absoluteDestinationPath, CancellationToken cancellationToken = default);
}