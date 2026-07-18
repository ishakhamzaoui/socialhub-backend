using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>A single media attachment as shown to API callers — reuses MediaDto's URL-not-storage-path convention (spec §22) rather than exposing MediaAsset.StoragePath.</summary>
public sealed record PostMediaSummaryDto(
    Guid MediaAssetId,
    int Order,
    MediaKind Kind,
    string Url,
    string? ThumbnailUrl);
 
public sealed record PostDto(
    Guid Id,
    Guid AuthorId,
    string? Content,
    PostType Type,
    Guid? OriginalPostId,
    PostVisibility Visibility,
    PostStatus Status,
    DateTime? ScheduledForUtc,
    DateTime? PublishedAtUtc,
    bool IsPinned,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<PostMediaSummaryDto> Media,
    IReadOnlyList<string> Hashtags,
    IReadOnlyList<Guid> MentionedUserIds);