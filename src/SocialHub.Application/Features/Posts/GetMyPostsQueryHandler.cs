using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class GetMyPostsQueryHandler : IQueryHandler<GetMyPostsQuery, PagedPostListDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostReactionRepository _postReactionRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserProfileRepository _userProfileRepository;
 
    public GetMyPostsQueryHandler(
        ICurrentUserService currentUserService,
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
        _postRepository = postRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _commentRepository = commentRepository;
        _postReactionRepository = postReactionRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _userProfileRepository = userProfileRepository;
    }
 
    public async Task<Result<PagedPostListDto>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<PagedPostListDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var (posts, total) = await _postRepository.GetByAuthorAsync(authorId, request.Status, request.Page, request.PageSize, cancellationToken);
 
        var dtos = new List<PostDto>();
        foreach (var post in posts)
        {
            dtos.Add(await PostDtoFactory.CreateAsync(
                post, authorId, _mediaAssetRepository, _hashtagRepository, _commentRepository,
                _postReactionRepository, _postRepository, _followRepository, _userBlockRepository,
                _userProfileRepository, cancellationToken));
        }
 
        return Result.Success(new PagedPostListDto(dtos, total, request.Page, request.PageSize));
    }
}