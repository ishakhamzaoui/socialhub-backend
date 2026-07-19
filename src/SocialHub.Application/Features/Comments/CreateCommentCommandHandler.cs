using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CommentDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public CreateCommentCommandHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<CommentDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        // Comment inherits the parent Post's visibility wholesale (confirmed
        // decision, Phase 7 kickoff) — reuse Phase 6's PostAccessPolicy
        // as-is, no new policy class.
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, authorId, cancellationToken);
        if (access == PostAccessResult.Blocked)
        {
            return Result.Failure<CommentDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (access == PostAccessResult.Denied)
        {
            return Result.Failure<CommentDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        // Flagged assumption (Phase 7 kickoff, not explicitly asked):
        // comments only make sense on a live post — a draft/scheduled/
        // archived post doesn't accept comments, even from its own author.
        if (post.Status != PostStatus.Published)
        {
            return Result.Failure<CommentDto>(Error.Conflict("Post.NotPublished", "Comments can only be added to a published post."));
        }
 
        if (request.ParentCommentId is { } parentId)
        {
            var parent = await _commentRepository.GetByIdAsync(parentId, cancellationToken);
            if (parent is null || parent.PostId != request.PostId)
            {
                return Result.Failure<CommentDto>(Error.NotFound("Comment.NotFound", "The comment you're replying to doesn't exist."));
            }
        }
 
        var comment = Comment.Create(request.PostId, authorId, request.Content, request.ParentCommentId);
        await _commentRepository.AddAsync(comment, cancellationToken);
 
        if (request.MentionedUserIds is { Count: > 0 })
        {
            foreach (var userId in request.MentionedUserIds)
            {
                // Silently skip anyone with no profile — same reasoning as
                // Post's mention-attachment step in Phase 6.
                var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
                if (profile is not null)
                {
                    comment.AddMention(userId);
                }
            }
        }
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await CommentDtoFactory.CreateAsync(comment, authorId, _commentRepository, _commentReactionRepository, cancellationToken);
        return Result.Success(dto);
    }
}