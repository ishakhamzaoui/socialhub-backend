using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class ReactToCommentCommandHandler : ICommandHandler<ReactToCommentCommand, CommentDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public ReactToCommentCommandHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<CommentDto>> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<CommentDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        // GetByIdWithDetailsAsync (not the base GetByIdAsync) — Mentions
        // must already be loaded before CommentDtoFactory builds the
        // returned DTO at the end of this handler.
        var comment = await _commentRepository.GetByIdWithDetailsAsync(request.CommentId, cancellationToken);
        if (comment is null || comment.IsDeleted)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId, cancellationToken);
        if (access == PostAccessResult.Blocked)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (access == PostAccessResult.Denied)
        {
            return Result.Failure<CommentDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        // Single-comment-level blocking (confirmed decision #5).
        if (await _userBlockRepository.IsBlockedEitherDirectionAsync(comment.AuthorId, requesterId, cancellationToken))
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var existing = await _commentReactionRepository.GetAsync(comment.Id, requesterId, cancellationToken);
        if (existing is null)
        {
            var reaction = CommentReaction.Create(comment.Id, requesterId, request.Type);
            await _commentReactionRepository.AddAsync(reaction, cancellationToken);
        }
        else
        {
            existing.ChangeType(request.Type);
        }
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await CommentDtoFactory.CreateAsync(comment, requesterId, _commentRepository, _commentReactionRepository, cancellationToken);
        return Result.Success(dto);
    }
}