using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>
/// Maps a Post aggregate (which only stores bare MediaAssetId/HashtagId
/// references — see script 31) into the API-facing PostDto, resolving
/// those references via the same repositories the Media and Hashtags
/// features already use. Shared by Create/Update (this script) and
/// GetPost (a later script) so this lookup-and-map logic exists in exactly
/// one place.
/// </summary>
public static class PostDtoFactory
{
    public static async Task<PostDto> CreateAsync(
        Post post,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
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
            mentionedUserIds);
    }
 
    private static PostMediaSummaryDto ToSummary(MediaAsset asset, int order) => new(
        asset.Id,
        order,
        asset.Kind,
        $"/api/v1/media/{asset.Id}/download",
        asset.ThumbnailStoragePath is not null ? $"/api/v1/media/{asset.Id}/thumbnail" : null);
}