using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class RemovePostReactionCommandHandler : ICommandHandler<RemovePostReactionCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostReactionRepository _postReactionRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public RemovePostReactionCommandHandler(
        ICurrentUserService currentUserService,
        IPostReactionRepository postReactionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postReactionRepository = postReactionRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(RemovePostReactionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var existing = await _postReactionRepository.GetAsync(request.PostId, requesterId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("Reaction.NotFound", "You haven't reacted to this post."));
        }
 
        _postReactionRepository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}