using SocialHub.Domain.Media;
 
namespace SocialHub.Application.Features.Media;
 
/// <summary>
/// Public-facing media metadata. Deliberately does NOT expose
/// MediaAsset.StoragePath/ThumbnailStoragePath (internal filesystem detail —
/// spec §22) — callers get stable API URLs instead (roadmap 4.1 exit
/// criterion: "retrieved via a stable URL").
/// </summary>
public sealed record MediaDto(
    Guid Id,
    MediaKind Kind,
    MediaCategory Category,
    string OriginalFileName,
    long SizeBytes,
    int? WidthPx,
    int? HeightPx,
    double? DurationSeconds,
    string Url,
    string? ThumbnailUrl,
    DateTime CreatedAtUtc)
{
    public static MediaDto From(MediaAsset asset) => new(
        asset.Id,
        asset.Kind,
        asset.Category,
        asset.OriginalFileName,
        asset.SizeBytes,
        asset.WidthPx,
        asset.HeightPx,
        asset.DurationSeconds,
        $"/api/v1/media/{asset.Id}/download",
        asset.ThumbnailStoragePath is not null ? $"/api/v1/media/{asset.Id}/thumbnail" : null,
        asset.CreatedAtUtc);
}