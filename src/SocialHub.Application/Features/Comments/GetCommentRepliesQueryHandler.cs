using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class GetCommentRepliesQueryHandler : IQueryHandler<GetCommentRepliesQuery, PagedCommentListDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetCommentRepliesQueryHandler(
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
 
    public async Task<Result<PagedCommentListDto>> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<PagedCommentListDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var parent = await _commentRepository.GetByIdAsync(request.ParentCommentId, cancellationToken);
        if (parent is null)
        {
            return Result.Failure<PagedCommentListDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var post = await _postRepository.GetByIdAsync(parent.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PagedCommentListDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId, cancellationToken);
        if (access == PostAccessResult.Blocked)
        {
            return Result.Failure<PagedCommentListDto>(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (access == PostAccessResult.Denied)
        {
            return Result.Failure<PagedCommentListDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        var blockedByRequester = await _userBlockRepository.GetBlockedUserIdsAsync(requesterId, cancellationToken);
        var blockingRequester = await _userBlockRepository.GetBlockedByUserIdsAsync(requesterId, cancellationToken);
        var excludeAuthorIds = blockedByRequester.Concat(blockingRequester).Distinct().ToList();
 
        var (replies, total) = await _commentRepository.GetRepliesAsync(request.ParentCommentId, request.Page, request.PageSize, excludeAuthorIds, cancellationToken);
 
        var dtos = new List<CommentDto>(replies.Count);
        foreach (var reply in replies)
        {
            dtos.Add(await CommentDtoFactory.CreateAsync(reply, requesterId, _commentRepository, _commentReactionRepository, cancellationToken));
        }
 
        return Result.Success(new PagedCommentListDto(dtos, total, request.Page, request.PageSize));
    }
}