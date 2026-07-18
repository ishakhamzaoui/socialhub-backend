using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class RepostCommandHandler : IRequestHandler<RepostCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IPostRepostRepository _postRepostRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public RepostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IPostRepostRepository postRepostRepository,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _postRepostRepository = postRepostRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(RepostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var original = await _postRepository.GetByIdAsync(request.OriginalPostId, cancellationToken);
 
        // NotFound whether the post genuinely doesn't exist, isn't
        // published, or a block exists between the two authors — same
        // non-leaking convention as CreatePostCommandHandler's quote check.
        if (original is null || original.Status != PostStatus.Published)
        {
            return Result.Failure(Error.NotFound("Post.NotFound", "The post you're trying to repost doesn't exist."));
        }
 
        if (await _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, original.AuthorId, cancellationToken))
        {
            return Result.Failure(Error.NotFound("Post.NotFound", "The post you're trying to repost doesn't exist."));
        }
 
        if (await _postRepostRepository.ExistsAsync(requesterId, request.OriginalPostId, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Repost.AlreadyExists", "You've already reposted this post."));
        }
 
        var repost = PostRepost.Create(requesterId, request.OriginalPostId);
        await _postRepostRepository.AddAsync(repost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}