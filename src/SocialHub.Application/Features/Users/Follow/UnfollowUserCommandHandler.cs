using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class UnfollowUserCommandHandler : ICommandHandler<UnfollowUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UnfollowUserCommandHandler(
        ICurrentUserService currentUserService,
        IFollowRepository followRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _followRepository = followRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var follow = await _followRepository.GetAsync(requesterId, request.TargetUserId, cancellationToken);
        if (follow is null)
        {
            return Result.Failure(Error.NotFound("Follow.NotFound", "You are not following this user."));
        }
 
        follow.MarkUnfollowed();
        _followRepository.Remove(follow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}