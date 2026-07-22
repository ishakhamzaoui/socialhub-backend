using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Flagged assumption (Phase 7 kickoff, not explicitly asked): pinning is
/// restricted to the POST's author, not the comment's own author — mirrors
/// how mainstream platforms let a post owner curate a top comment. Revisit
/// if comment-authors were meant to have self-pin rights instead.
/// </summary>
public sealed class PinCommentCommandHandler : ICommandHandler<PinCommentCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public PinCommentCommandHandler(
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
 
    public async Task<Result> Handle(PinCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null || comment.IsDeleted)
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
            return Result.Failure(Error.Forbidden("Comment.PinForbidden", "Only the post's author can pin a comment."));
        }
 
        var currentlyPinned = await _commentRepository.GetPinnedCommentForPostAsync(post.Id, cancellationToken);
        if (currentlyPinned is not null && currentlyPinned.Id != comment.Id)
        {
            currentlyPinned.Unpin();
        }
 
        comment.Pin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}