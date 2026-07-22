using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class GetPostQueryHandler : IQueryHandler<GetPostQuery, PostDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostReactionRepository _postReactionRepository;
    private readonly IUserProfileRepository _userProfileRepository;
 
    public GetPostQueryHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        ICommentRepository commentRepository,
        IPostReactionRepository postReactionRepository,
        IUserProfileRepository userProfileRepository)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _commentRepository = commentRepository;
        _postReactionRepository = postReactionRepository;
        _userProfileRepository = userProfileRepository;
    }
 
    public async Task<Result<PostDto>> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<PostDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var post = await _postRepository.GetByIdWithDetailsAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId, cancellationToken);
        switch (access)
        {
            case PostAccessResult.Blocked:
                // Deliberately NotFound, not Forbidden — never reveal a block's existence.
                return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
            case PostAccessResult.Denied:
                return Result.Failure<PostDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        var dto = await PostDtoFactory.CreateAsync(
            post, requesterId, _mediaAssetRepository, _hashtagRepository, _commentRepository,
            _postReactionRepository, _postRepository, _followRepository, _userBlockRepository,
            _userProfileRepository, cancellationToken);
        return Result.Success(dto);
    }
}