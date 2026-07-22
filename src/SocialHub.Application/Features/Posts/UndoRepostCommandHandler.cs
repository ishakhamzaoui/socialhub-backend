using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class UndoRepostCommandHandler : ICommandHandler<UndoRepostCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepostRepository _postRepostRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UndoRepostCommandHandler(ICurrentUserService currentUserService, IPostRepostRepository postRepostRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepostRepository = postRepostRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UndoRepostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        // Safe no-op if no repost row exists (IPostRepostRepository.RemoveAsync's documented behavior).
        await _postRepostRepository.RemoveAsync(requesterId, request.OriginalPostId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}