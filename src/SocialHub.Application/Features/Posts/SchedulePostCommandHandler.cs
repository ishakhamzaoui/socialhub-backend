using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class SchedulePostCommandHandler : ICommandHandler<SchedulePostCommand, PostDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public SchedulePostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<PostDto>> Handle(SchedulePostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<PostDto>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var post = await _postRepository.GetByIdForAuthorAsync(request.PostId, authorId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PostDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (post.Status is not (PostStatus.Draft or PostStatus.Scheduled))
        {
            return Result.Failure<PostDto>(Error.Conflict("Post.CannotSchedule", $"Cannot schedule a post in {post.Status} status."));
        }
 
        try
        {
            post.Schedule(request.ScheduledForUtc);
        }
        catch (ArgumentException ex)
        {
            // Belt-and-braces alongside the validator's own future-date rule.
            return Result.Failure<PostDto>(Error.Validation("Post.Invalid", ex.Message));
        }
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await PostDtoFactory.CreateAsync(post, _mediaAssetRepository, _hashtagRepository, cancellationToken);
        return Result.Success(dto);
    }
}