using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
using MediatR;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class ReportCommentCommandHandler : IRequestHandler<ReportCommentCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReportRepository _commentReportRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public ReportCommentCommandHandler(
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ICommentReportRepository commentReportRepository,
        IPostRepository postRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _commentReportRepository = commentReportRepository;
        _postRepository = postRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(ReportCommentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var reporterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (comment.AuthorId == reporterId)
        {
            return Result.Failure(Error.Validation("Comment.CannotReportOwn", "You cannot report your own comment."));
        }
 
        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        var access = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, reporterId, cancellationToken);
        if (access == PostAccessResult.Blocked)
        {
            return Result.Failure(Error.NotFound("Comment.NotFound", "Comment not found."));
        }
 
        if (access == PostAccessResult.Denied)
        {
            return Result.Failure(Error.Forbidden("Post.Private", "This post is not visible to you."));
        }
 
        if (await _commentReportRepository.ExistsAsync(comment.Id, reporterId, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Comment.AlreadyReported", "You have already reported this comment."));
        }
 
        var report = CommentReport.Create(comment.Id, reporterId, request.Reason, request.Details);
        await _commentReportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}