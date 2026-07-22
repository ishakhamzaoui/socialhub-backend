using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class RemoveCommentReactionCommandHandler : ICommandHandler<RemoveCommentReactionCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public RemoveCommentReactionCommandHandler(
        ICurrentUserService currentUserService,
        ICommentReactionRepository commentReactionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentReactionRepository = commentReactionRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(RemoveCommentReactionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var existing = await _commentReactionRepository.GetAsync(request.CommentId, requesterId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("Reaction.NotFound", "You haven't reacted to this comment."));
        }
 
        _commentReactionRepository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}