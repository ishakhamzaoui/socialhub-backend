using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Features.Users.Follow;
using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>
/// Maps a Post aggregate into the API-facing PostDto. Shared by every
/// Post-returning handler/query so this lookup-and-map logic exists in
/// exactly one place.
///
/// SIGNATURE CHANGE (script 52, Phase 8): CreateAsync now takes the
/// requester's Guid plus six repositories instead of two, to build
/// CommentCount, per-type reaction counts, the requester's own reaction, and
/// an optional QuotedPost preview. This is a confirmed scope decision, not
/// scope creep — see script 52's header. Every existing caller was updated
/// in the same script.
/// </summary>
public static class PostDtoFactory
{
    private const int QuotedContentSnippetMaxLength = 200;
 
    public static async Task<PostDto> CreateAsync(
        Post post,
        Guid requesterId,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        ICommentRepository commentRepository,
        IPostReactionRepository postReactionRepository,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        CancellationToken cancellationToken = default)
    {
        var media = new List<PostMediaSummaryDto>();
        foreach (var postMedia in post.Media.OrderBy(m => m.Order))
        {
            var asset = await mediaAssetRepository.GetByIdAsync(postMedia.MediaAssetId, cancellationToken);
            if (asset is null)
            {
                // Same dangling-reference note as OriginalPostId (see this
                // script's header) — a MediaAsset could theoretically be
                // gone if something bypassed DeleteMediaCommand's normal
                // path. Skip it rather than fail the whole DTO.
                continue;
            }
 
            media.Add(ToSummary(asset, postMedia.Order));
        }
 
        var hashtags = new List<string>();
        foreach (var postHashtag in post.Hashtags)
        {
            var hashtag = await hashtagRepository.GetByIdAsync(postHashtag.HashtagId, cancellationToken);
            if (hashtag is not null)
            {
                hashtags.Add(hashtag.Tag);
            }
        }
 
        var mentionedUserIds = post.Mentions.Select(m => m.MentionedUserId).ToList();
 
        var commentCount = await commentRepository.GetTotalCommentCountAsync(post.Id, cancellationToken);
        var reactionCounts = await postReactionRepository.GetCountsByTypeAsync(post.Id, cancellationToken);
        var myReaction = await postReactionRepository.GetAsync(post.Id, requesterId, cancellationToken);
 
        QuotedPostPreviewDto? quotedPost = null;
        if (post.Type == PostType.Quote && post.OriginalPostId is not null)
        {
            quotedPost = await BuildQuotedPreviewAsync(
                post.OriginalPostId.Value,
                requesterId,
                postRepository,
                followRepository,
                userBlockRepository,
                userProfileRepository,
                cancellationToken);
        }
 
        return new PostDto(
            post.Id,
            post.AuthorId,
            post.Content,
            post.Type,
            post.OriginalPostId,
            post.Visibility,
            post.Status,
            post.ScheduledForUtc,
            post.PublishedAtUtc,
            post.IsPinned,
            post.CreatedAtUtc,
            post.UpdatedAtUtc,
            media,
            hashtags,
            mentionedUserIds,
            commentCount,
            reactionCounts,
            myReaction?.Type,
            quotedPost);
    }
 
    /// <summary>
    /// Returns null — never throws, never fails the containing PostDto —
    /// whenever the quoted original is gone (no DB-level FK backs
    /// Post.OriginalPostId, per Phase 6's known limitation) or is no longer
    /// visible to the requester (reuses PostAccessPolicy unmodified;
    /// Blocked and Denied both suppress the preview, matching the existing
    /// non-leaking convention rather than distinguishing them here).
    /// </summary>
    private static async Task<QuotedPostPreviewDto?> BuildQuotedPreviewAsync(
        Guid originalPostId,
        Guid requesterId,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        CancellationToken cancellationToken)
    {
        var original = await postRepository.GetByIdAsync(originalPostId, cancellationToken);
        if (original is null)
        {
            return null;
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(followRepository, userBlockRepository, original, requesterId, cancellationToken);
        if (access is PostAccessResult.Blocked or PostAccessResult.Denied)
        {
            return null;
        }
 
        var authorProfile = await userProfileRepository.GetByUserIdAsync(original.AuthorId, cancellationToken);
 
        return new QuotedPostPreviewDto(
            original.Id,
            original.AuthorId,
            authorProfile is not null ? UserSummaryDto.From(authorProfile) : null,
            TruncateSnippet(original.Content),
            original.CreatedAtUtc);
    }
 
    private static string? TruncateSnippet(string? content)
    {
        if (content is null)
        {
            return null;
        }
 
        return content.Length <= QuotedContentSnippetMaxLength
            ? content
            : content[..QuotedContentSnippetMaxLength] + "…";
    }
 
    private static PostMediaSummaryDto ToSummary(MediaAsset asset, int order) => new(
        asset.Id,
        order,
        asset.Kind,
        $"/api/v1/media/{asset.Id}/download",
        asset.ThumbnailStoragePath is not null ? $"/api/v1/media/{asset.Id}/thumbnail" : null);
}