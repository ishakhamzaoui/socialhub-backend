namespace SocialHub.Infrastructure.Storage;
 
/// <summary>
/// Bound from the "Storage" appsettings section. RootPath defaults to the
/// exact layout mandated by spec §22 — override only if a given deployment
/// genuinely needs a different mount point, never to work around a missing
/// directory (create the directory instead; LocalFileStorageService also
/// creates it defensively on startup).
/// </summary>
public sealed class StorageOptions
{
    public string RootPath { get; set; } = "/var/socialhub/uploads";
 
    /// <summary>How long a file may sit under temp/ before MediaCleanupService deletes it (roadmap 4.7).</summary>
    public int TempFileTtlHours { get; set; } = 24;
 
    /// <summary>Longest-side cap, in pixels, applied when resizing/compressing an uploaded image.</summary>
    public int ImageMaxDimension { get; set; } = 2048;
 
    /// <summary>Longest-side cap, in pixels, for generated thumbnails (images and video frames alike).</summary>
    public int ThumbnailSize { get; set; } = 320;
}