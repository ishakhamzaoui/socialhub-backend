using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class UnpinPostCommandHandler : ICommandHandler<UnpinPostCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UnpinPostCommandHandler(ICurrentUserService currentUserService, IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UnpinPostCommand request, CancellationToken cancellationToken)
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
 
        post.Unpin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}