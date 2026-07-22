using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Pagination;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Feed;
 
public sealed class GetFollowingFeedQueryHandler : IQueryHandler<GetFollowingFeedQuery, CursorPagedFeedDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFeedRepository _feedRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostReactionRepository _postReactionRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserProfileRepository _userProfileRepository;
 
    public GetFollowingFeedQueryHandler(
        ICurrentUserService currentUserService,
        IFeedRepository feedRepository,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        ICommentRepository commentRepository,
        IPostReactionRepository postReactionRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository)
    {
        _currentUserService = currentUserService;
        _feedRepository = feedRepository;
        _postRepository = postRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _commentRepository = commentRepository;
        _postReactionRepository = postReactionRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _userProfileRepository = userProfileRepository;
    }
 
    public async Task<Result<CursorPagedFeedDto>> Handle(GetFollowingFeedQuery request, CancellationToken cancellationToken)
    {
        // Belt-and-braces (see script 56's header) — RequesterId is trusted
        // input on the request object only because FeedController set it
        // from the authenticated principal; this independently confirms it.
        if (!Guid.TryParse(_currentUserService.UserId, out var authenticatedUserId) || authenticatedUserId != request.RequesterId)
        {
            return Result.Failure<CursorPagedFeedDto>(Error.Forbidden("Feed.RequesterMismatch", "You can only request your own feed."));
        }
 
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var cursor = FeedCursor.TryDecode(request.Cursor, out var decoded) ? decoded : (FeedCursor?)null;
 
        var (entries, hasMore) = await _feedRepository.GetFollowingFeedAsync(request.RequesterId, cursor, pageSize, cancellationToken);
 
        var items = await FeedItemDtoFactory.CreateAsync(
            entries, request.RequesterId, _postRepository, _mediaAssetRepository, _hashtagRepository,
            _commentRepository, _postReactionRepository, _followRepository, _userBlockRepository,
            _userProfileRepository, cancellationToken);
 
        var nextCursor = hasMore && entries.Count > 0
            ? new FeedCursor(entries[^1].SortKey, entries[^1].PostId).Encode()
            : null;
 
        return Result.Success(new CursorPagedFeedDto(items, nextCursor, hasMore));
    }
}