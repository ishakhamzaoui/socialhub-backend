using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
// ICommandHandler<TCommand> (Application/Common/Messaging/ICommandHandler.cs)
// — the codebase's canonical shape for void-returning commands, matching
// Phase 5's BlockUserCommandHandler/MuteUserCommandHandler et al. Script 50
// (Phase 8 kickoff) corrected this handler from a direct
// IRequestHandler<TCommand, Result> implementation after discovering the
// wrapper genuinely exists — see the Advancement doc's Phase 8 entry.
public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public DeletePostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var post = await _postRepository.GetByIdForAuthorAsync(request.PostId, authorId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));
        }
 
        // Hard-deletes the Post row (PostMedia/PostHashtag/PostMention
        // cascade via FK). Does NOT delete the underlying MediaAsset rows
        // — same "history is retained, nothing auto-deletes a MediaAsset
        // except the explicit DeleteMediaCommand" precedent as avatar/cover
        // replacement (Phase 5).
        post.MarkDeleted();
        _postRepository.Remove(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}