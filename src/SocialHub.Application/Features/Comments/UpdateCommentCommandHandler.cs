using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand, CommentDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UpdateCommentCommandHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<CommentDto>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<CommentDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var comment = await _commentRepository.GetByIdForAuthorAsync(request.CommentId, authorId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (comment.IsDeleted)
        {
            return Result.Failure<CommentDto>(Error.Conflict("Comment.Deleted", "A deleted comment cannot be edited."));
        }
 
        comment.UpdateContent(request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await CommentDtoFactory.CreateAsync(comment, authorId, _commentRepository, _commentReactionRepository, cancellationToken);
        return Result.Success(dto);
    }
}