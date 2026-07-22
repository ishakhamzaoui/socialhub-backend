using SocialHub.Application.Features.Users.Follow;
using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>A single media attachment as shown to API callers — reuses MediaDto's URL-not-storage-path convention (spec §22) rather than exposing MediaAsset.StoragePath.</summary>
public sealed record PostMediaSummaryDto(
    Guid MediaAssetId,
    int Order,
    MediaKind Kind,
    string Url,
    string? ThumbnailUrl);
 
/// <summary>
/// Added in script 52 (Phase 8) for quote/repost preview. Deliberately NOT a
/// full nested PostDto (which would recurse into another QuotedPost, another
/// CommentCount, etc. for no real benefit) — just enough to render a
/// "quoting: ..." card. Built by PostDtoFactory, which returns null rather
/// than failing the whole PostDto whenever the original is gone, deleted, or
/// not visible to the current requester (see PostDtoFactory's remarks).
/// </summary>
public sealed record QuotedPostPreviewDto(
    Guid PostId,
    Guid AuthorId,
    UserSummaryDto? Author,
    string? ContentSnippet,
    DateTime CreatedAtUtc);
 
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
    IReadOnlyList<Guid> MentionedUserIds,
    int CommentCount,
    IReadOnlyDictionary<ReactionType, int> ReactionCounts,
    ReactionType? MyReaction,
    QuotedPostPreviewDto? QuotedPost);