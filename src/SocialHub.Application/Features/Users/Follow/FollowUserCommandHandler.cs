using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class FollowUserCommandHandler : ICommandHandler<FollowUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public FollowUserCommandHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        if (requesterId == request.TargetUserId)
        {
            return Result.Failure(Error.Validation("Follow.CannotFollowSelf", "You cannot follow yourself."));
        }
 
        var targetProfile = await _userProfileRepository.GetByUserIdAsync(request.TargetUserId, cancellationToken);
        if (targetProfile is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        if (await _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, request.TargetUserId, cancellationToken))
        {
            // Deliberately NotFound, not Forbidden — same block-non-disclosure rule as ProfileAccessPolicy.
            return Result.Failure(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        if (await _followRepository.ExistsAsync(requesterId, request.TargetUserId, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Follow.AlreadyFollowing", "You are already following this user."));
        }
 
        var follow = Domain.Users.Follow.Create(requesterId, request.TargetUserId);
        await _followRepository.AddAsync(follow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}