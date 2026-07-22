using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class UnpinCommentCommandHandler : ICommandHandler<UnpinCommentCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UnpinCommentCommandHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UnpinCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (post.AuthorId != requesterId)
        {
            return Result.Failure(Error.Forbidden("Comment.PinForbidden", "Only the post's author can unpin a comment."));
        }
 
        comment.Unpin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}