using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class PinPostCommandHandler : ICommandHandler<PinPostCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public PinPostCommandHandler(ICurrentUserService currentUserService, IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(PinPostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var post = await _postRepository.GetByIdForAuthorAsync(request.PostId, authorId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (post.Status != PostStatus.Published)
        {
            return Result.Failure(Error.Conflict("Post.NotPublished", "Only a published post can be pinned."));
        }
 
        var currentlyPinned = await _postRepository.GetPinnedPostAsync(authorId, cancellationToken);
        if (currentlyPinned is not null && currentlyPinned.Id != post.Id)
        {
            currentlyPinned.Unpin();
        }
 
        post.Pin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}