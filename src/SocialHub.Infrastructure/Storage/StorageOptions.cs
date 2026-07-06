namespace SocialHub.Infrastructure.Storage;
 
/// <summary>
/// Bound from the "Storage" appsettings section. RootPath defaults to the
/// exact layout mandated by spec §22 — override only if a given deployment
/// genuinely needs a different mount point, never to work around a missing
/// directory (create the directory instead; LocalFileStorageService also
/// creates it defensively on startup).
///
/// Image/video resize and thumbnail dimensions are NOT configured here —
/// they're supplied as explicit call parameters by whichever Application
/// handler invokes IImageProcessingService/IVideoProcessingService (see
/// UploadMediaCommandHandler), since Application cannot reference this
/// (Infrastructure) project to read them from here.
/// </summary>
public sealed class StorageOptions
{
    public string RootPath { get; set; } = "/var/socialhub/uploads";
 
    /// <summary>How long a file may sit under temp/ before MediaCleanupService deletes it (roadmap 4.7).</summary>
    public int TempFileTtlHours { get; set; } = 24;
}