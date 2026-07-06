namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Abstraction over the native filesystem layout defined in spec §22
/// (/var/socialhub/uploads/{users,posts,messages,communities,temp,thumbnails}).
/// Implemented by SocialHub.Infrastructure.Storage.LocalFileStorageService.
///
/// Every upload is written to the "temp" area first via SaveToTempAsync, then
/// (only after processing and the MediaAsset row have both succeeded) moved
/// to its final destination via PromoteAsync. This two-step flow is what
/// gives the Phase 4 cleanup service (roadmap 4.7) real, well-defined work:
/// anything left under temp/ past a short TTL is by definition an
/// interrupted or abandoned upload and is safe to delete.
///
/// All paths accepted/returned by this interface other than the Get*AbsolutePath
/// methods are relative to the configured storage root — never absolute —
/// so relative paths are safe to persist in MediaAsset.StoragePath.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Writes the stream to a new temp file and returns its relative path (e.g. "temp/{guid}.jpg").</summary>
    Task<string> SaveToTempAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default);
 
    /// <summary>Moves a temp file (and, if provided, its temp thumbnail) to their final category folders. Returns the final relative paths.</summary>
    Task<(string FinalPath, string? FinalThumbnailPath)> PromoteAsync(
        string tempRelativePath,
        string? tempThumbnailRelativePath,
        Guid ownerId,
        string category,
        string fileName,
        CancellationToken cancellationToken = default);
 
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
 
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
 
    /// <summary>Resolves a relative path to a full filesystem path — used internally by image/video processors, which need a real path to hand to SkiaSharp/ffprobe/ffmpeg.</summary>
    string GetAbsolutePath(string relativePath);
 
    /// <summary>Lists relative paths of every file directly under the temp folder whose last-write time is older than the given UTC cutoff. Used by the cleanup service (roadmap 4.7).</summary>
    Task<IReadOnlyList<string>> ListTempFilesOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}