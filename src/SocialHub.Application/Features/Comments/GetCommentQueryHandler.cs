using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class GetCommentQueryHandler : IQueryHandler<GetCommentQuery, CommentDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetCommentQueryHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
    }
 
    public async Task<Result<CommentDto>> Handle(GetCommentQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<CommentDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var comment = await _commentRepository.GetByIdWithDetailsAsync(request.CommentId, cancellationToken);
        if (comment is null)
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
 
        // Single-comment-level blocking (confirmed decision #5): the
        // comment's own author can be blocked/blocking independently of the
        // post's author.
        if (await _userBlockRepository.IsBlockedEitherDirectionAsync(comment.AuthorId, requesterId, cancellationToken))
        {
            return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var dto = await CommentDtoFactory.CreateAsync(comment, requesterId, _commentRepository, _commentReactionRepository, cancellationToken);
        return Result.Success(dto);
    }
}