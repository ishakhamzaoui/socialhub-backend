using SocialHub.Application.Features.Posts;
using SocialHub.Application.Features.Users.Follow;
 
namespace SocialHub.Application.Features.Feed;
 
/// <summary>
/// One rendered feed row. RepostedByUserId/RepostedBy are both null for a
/// direct post/quote entry; both set when this row represents someone
/// reposting Post. FeedTimestampUtc is "when this appeared in the feed"
/// (the post's own CreatedAtUtc for a direct entry, the repost's
/// CreatedAtUtc for a repost entry) — always a real timestamp for display,
/// even for the trending feed, where it is NOT what the feed is ordered by
/// (see IFeedRepository.FeedEntry's remarks).
/// </summary>
public sealed record FeedItemDto(
    PostDto Post,
    Guid? RepostedByUserId,
    UserSummaryDto? RepostedBy,
    DateTime FeedTimestampUtc);
 
public sealed record CursorPagedFeedDto(IReadOnlyList<FeedItemDto> Items, string? NextCursor, bool HasMore);