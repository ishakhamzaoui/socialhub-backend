using SocialHub.Domain.Common;
using SocialHub.Domain.Media.Events;
 
namespace SocialHub.Domain.Media;
 
/// <summary>
/// A single uploaded media file's metadata (spec §15.11, §22). The physical
/// bytes live on the filesystem; this row is always the source of truth for
/// path/size/MIME/owner — never inferred from disk state (spec §22, "File
/// metadata is always persisted in PostgreSQL, never inferred from disk
/// state").
///
/// OwnerId is a bare Guid (ApplicationUser.Id), not a navigation property:
/// Domain cannot reference SocialHub.Identity, and the Domain "User"
/// aggregate doesn't exist until Phase 5 (see
/// SocialHub-Developer-Onboarding.md §7 — the same pattern RefreshToken.UserId
/// already uses).
///
/// Lifecycle / why there's no Status field: a MediaAsset row is only ever
/// inserted after upload validation, processing (dimension/duration
/// extraction, resize, thumbnail generation), AND promotion from the
/// filesystem's temp staging area to its final Category folder have all
/// fully succeeded. If any step fails, nothing is persisted — the leftover
/// temp file is swept later by the cleanup service (roadmap 4.7,
/// MediaCleanupService in Infrastructure). This keeps the entity itself
/// free of a partial/pending state to reason about; "temp" exists only as a
/// filesystem staging concept inside IFileStorageService, never as data
/// here.
/// </summary>
public sealed class MediaAsset : BaseEntity, IAggregateRoot
{
    private MediaAsset()
    {
        // Reserved for EF Core materialization.
    }
 
    private MediaAsset(
        Guid id,
        Guid ownerId,
        MediaKind kind,
        MediaCategory category,
        string originalFileName,
        string storagePath,
        string? thumbnailStoragePath,
        string mimeType,
        long sizeBytes,
        int? widthPx,
        int? heightPx,
        double? durationSeconds)
        : base(id)
    {
        OwnerId = ownerId;
        Kind = kind;
        Category = category;
        OriginalFileName = originalFileName;
        StoragePath = storagePath;
        ThumbnailStoragePath = thumbnailStoragePath;
        MimeType = mimeType;
        SizeBytes = sizeBytes;
        WidthPx = widthPx;
        HeightPx = heightPx;
        DurationSeconds = durationSeconds;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid OwnerId { get; private set; }
 
    public MediaKind Kind { get; private set; }
 
    public MediaCategory Category { get; private set; }
 
    public string OriginalFileName { get; private set; } = default!;
 
    /// <summary>Relative path from the storage root (spec §22), e.g. "users/{ownerId}/{guid}.jpg".</summary>
    public string StoragePath { get; private set; } = default!;
 
    /// <summary>Relative path under the shared "thumbnails/" folder, if one was generated.</summary>
    public string? ThumbnailStoragePath { get; private set; }
 
    public string MimeType { get; private set; } = default!;
 
    public long SizeBytes { get; private set; }
 
    public int? WidthPx { get; private set; }
 
    public int? HeightPx { get; private set; }
 
    public double? DurationSeconds { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static MediaAsset Create(
        Guid ownerId,
        MediaKind kind,
        MediaCategory category,
        string originalFileName,
        string storagePath,
        string? thumbnailStoragePath,
        string mimeType,
        long sizeBytes,
        int? widthPx = null,
        int? heightPx = null,
        double? durationSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new ArgumentException("Storage path cannot be empty.", nameof(storagePath));
        }
 
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new ArgumentException("MIME type cannot be empty.", nameof(mimeType));
        }
 
        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Size must be positive.");
        }
 
        var asset = new MediaAsset(
            Guid.NewGuid(),
            ownerId,
            kind,
            category,
            originalFileName,
            storagePath,
            thumbnailStoragePath,
            mimeType,
            sizeBytes,
            widthPx,
            heightPx,
            durationSeconds);
 
        asset.AddDomainEvent(new MediaUploadedEvent(asset.Id, ownerId, kind, category));
 
        return asset;
    }
 
    /// <summary>Raises MediaDeletedEvent; callers still remove the row via IRepository.Remove separately.</summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new MediaDeletedEvent(Id, OwnerId));
    }
}