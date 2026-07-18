using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
// Plain IRequestHandler<TCommand, Result>, matching DeleteMediaCommandHandler's
// pattern for void-returning commands (no non-generic ICommandHandler<TCommand>
// wrapper exists in this codebase).
public sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public DeletePostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
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
 
        // Hard-deletes the Post row (PostMedia/PostHashtag/PostMention
        // cascade via FK). Does NOT delete the underlying MediaAsset rows
        // — same "history is retained, nothing auto-deletes a MediaAsset
        // except the explicit DeleteMediaCommand" precedent as avatar/cover
        // replacement (Phase 5).
        post.MarkDeleted();
        _postRepository.Remove(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}