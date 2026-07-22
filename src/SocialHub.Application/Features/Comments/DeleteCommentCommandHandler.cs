using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
// ICommandHandler<TCommand> — see DeletePostCommandHandler's header comment
// for why this replaced the direct IRequestHandler<TCommand, Result> shape
// (script 50, Phase 8 kickoff).
public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public DeleteCommentCommandHandler(ICurrentUserService currentUserService, ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var comment = await _commentRepository.GetByIdForAuthorAsync(request.CommentId, authorId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (comment.IsDeleted)
        {
            return Result.Failure(Error.Conflict("Comment.Deleted", "Comment is already deleted."));
        }
 
        // Soft delete (flagged assumption — see Comment.MarkDeleted()'s
        // remarks). Unlike DeletePostCommandHandler, there is NO
        // repository.Remove() call here: the row survives so any replies
        // underneath it in the thread aren't orphaned.
        comment.MarkDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}