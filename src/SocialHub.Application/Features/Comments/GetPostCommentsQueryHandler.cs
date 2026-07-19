using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class GetPostCommentsQueryHandler : IQueryHandler<GetPostCommentsQuery, PagedCommentListDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetPostCommentsQueryHandler(
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
 
    public async Task<Result<PagedCommentListDto>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<PagedCommentListDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<PagedCommentListDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId, cancellationToken);
        if (access == PostAccessResult.Blocked)
        {
            return Result.Failure<PagedCommentListDto>(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        if (access == PostAccessResult.Denied)
        {
            return Result.Failure<PagedCommentListDto>(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        // Single-comment-level blocking (confirmed decision #5): exclude
        // comments authored by anyone blocking/blocked-by the requester,
        // applied inside the repository query (script 45's correction) so
        // TotalCount stays accurate.
        var blockedByRequester = await _userBlockRepository.GetBlockedUserIdsAsync(requesterId, cancellationToken);
        var blockingRequester = await _userBlockRepository.GetBlockedByUserIdsAsync(requesterId, cancellationToken);
        var excludeAuthorIds = blockedByRequester.Concat(blockingRequester).Distinct().ToList();
 
        var (comments, total) = await _commentRepository.GetTopLevelForPostAsync(request.PostId, request.Page, request.PageSize, excludeAuthorIds, cancellationToken);
 
        var dtos = new List<CommentDto>(comments.Count);
        foreach (var comment in comments)
        {
            dtos.Add(await CommentDtoFactory.CreateAsync(comment, requesterId, _commentRepository, _commentReactionRepository, cancellationToken));
        }
 
        return Result.Success(new PagedCommentListDto(dtos, total, request.Page, request.PageSize));
    }
}