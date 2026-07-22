using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class ReactToPostCommandHandler : ICommandHandler<ReactToPostCommand, PostDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IPostReactionRepository _postReactionRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public ReactToPostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IPostReactionRepository postReactionRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        ICommentRepository commentRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _postReactionRepository = postReactionRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _commentRepository = commentRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<PostDto>> Handle(ReactToPostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<PostDto>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        // Same visibility/block gate as GetPostQuery — you can't react to a
        // post you're not allowed to see, and a block never leaks as
        // anything other than NotFound.
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId, cancellationToken);
        switch (access)
        {
            case PostAccessResult.Blocked:
                return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
            case PostAccessResult.Denied:
                return Result.Failure<PostDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        if (post.Status != PostStatus.Published)
        {
            return Result.Failure<PostDto>(Error.Conflict("Post.NotPublished", "Only a published post can be reacted to."));
        }
 
        var existing = await _postReactionRepository.GetAsync(post.Id, requesterId, cancellationToken);
        if (existing is null)
        {
            var reaction = PostReaction.Create(post.Id, requesterId, request.Type);
            await _postReactionRepository.AddAsync(reaction, cancellationToken);
        }
        else
        {
            existing.ChangeType(request.Type);
        }
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await PostDtoFactory.CreateAsync(
            post, requesterId, _mediaAssetRepository, _hashtagRepository, _commentRepository,
            _postReactionRepository, _postRepository, _followRepository, _userBlockRepository,
            _userProfileRepository, cancellationToken);
        return Result.Success(dto);
    }
}