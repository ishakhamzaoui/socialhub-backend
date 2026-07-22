using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, PostDto>
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
    private readonly IUnitOfWork _unitOfWork;
 
    public UpdatePostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        ICommentRepository commentRepository,
        IPostReactionRepository postReactionRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork)
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
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<PostDto>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<PostDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var post = await _postRepository.GetByIdForAuthorAsync(request.PostId, authorId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (post.Status == PostStatus.Archived)
        {
            return Result.Failure<PostDto>(Error.Conflict("Post.Archived", "An archived post cannot be edited."));
        }
 
        if (string.IsNullOrWhiteSpace(request.Content) && post.Media.Count == 0)
        {
            return Result.Failure<PostDto>(Error.Validation("Post.Invalid", "A post needs Content, at least one media attachment, or both."));
        }
 
        post.UpdateContent(request.Content);
        post.ChangeVisibility(request.Visibility);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await PostDtoFactory.CreateAsync(
            post, authorId, _mediaAssetRepository, _hashtagRepository, _commentRepository,
            _postReactionRepository, _postRepository, _followRepository, _userBlockRepository,
            _userProfileRepository, cancellationToken);
        return Result.Success(dto);
    }
}