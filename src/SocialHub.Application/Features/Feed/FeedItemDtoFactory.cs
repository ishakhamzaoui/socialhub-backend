using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Application.Features.Users.Follow;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>
/// Maps a page of FeedEntry (from IFeedRepository) into FeedItemDto, shared
/// by all four feed query handlers so this logic exists in exactly one
/// place — same reasoning as PostDtoFactory/CommentDtoFactory being the one
/// shared mapping point for their own feature areas.
/// </summary>
public static class FeedItemDtoFactory
{
    public static async Task<IReadOnlyList<FeedItemDto>> CreateAsync(
        IReadOnlyList<FeedEntry> entries,
        Guid requesterId,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        ICommentRepository commentRepository,
        IPostReactionRepository postReactionRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        CancellationToken cancellationToken)
    {
        // Batch-resolve reposter profiles up front (one query for the whole
        // page) rather than one lookup per repost entry.
        var reposterIds = entries
            .Where(e => e.RepostedByUserId is not null)
            .Select(e => e.RepostedByUserId!.Value)
            .Distinct()
            .ToList();
 
        Dictionary<Guid, UserProfile> reposterProfiles = reposterIds.Count > 0
            ? (await userProfileRepository.GetByUserIdsAsync(reposterIds, cancellationToken)).ToDictionary(p => p.UserId)
            : new Dictionary<Guid, UserProfile>();
 
        var items = new List<FeedItemDto>(entries.Count);
 
        foreach (var entry in entries)
        {
            // KNOWN LIMITATION, flagged in script 56's header: this issues
            // several queries per post via PostDtoFactory. Accepted for
            // Phase 8 given the 30-second cache on every feed query.
            var post = await postRepository.GetByIdWithDetailsAsync(entry.PostId, cancellationToken);
            if (post is null)
            {
                // FeedRepository already checked visibility moments
                // earlier, but the post could theoretically have been
                // deleted in between — skip rather than fail the whole
                // page over one stale entry.
                continue;
            }
 
            var postDto = await PostDtoFactory.CreateAsync(
                post,
                requesterId,
                mediaAssetRepository,
                hashtagRepository,
                commentRepository,
                postReactionRepository,
                postRepository,
                followRepository,
                userBlockRepository,
                userProfileRepository,
                cancellationToken);
 
            UserSummaryDto? repostedBy = null;
            if (entry.RepostedByUserId is { } reposterId && reposterProfiles.TryGetValue(reposterId, out var reposterProfile))
            {
                repostedBy = UserSummaryDto.From(reposterProfile);
            }
 
            items.Add(new FeedItemDto(postDto, entry.RepostedByUserId, repostedBy, entry.FeedTimestampUtc));
        }
 
        return items;
    }
}