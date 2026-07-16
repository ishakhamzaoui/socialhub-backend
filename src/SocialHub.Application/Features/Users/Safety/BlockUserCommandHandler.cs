using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class BlockUserCommandHandler : ICommandHandler<BlockUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public BlockUserCommandHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IUserBlockRepository userBlockRepository,
        IFollowRepository followRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _userBlockRepository = userBlockRepository;
        _followRepository = followRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        if (requesterId == request.TargetUserId)
        {
            return Result.Failure(Error.Validation("Block.CannotBlockSelf", "You cannot block yourself."));
        }
 
        var targetProfile = await _userProfileRepository.GetByUserIdAsync(request.TargetUserId, cancellationToken);
        if (targetProfile is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        if (await _userBlockRepository.IsBlockedAsync(requesterId, request.TargetUserId, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Block.AlreadyBlocked", "You have already blocked this user."));
        }
 
        var block = Domain.Users.UserBlock.Create(requesterId, request.TargetUserId);
        await _userBlockRepository.AddAsync(block, cancellationToken);
 
        // Confirmed decision: blocking severs any existing follow
        // relationship between the two users, in either direction.
        await _followRepository.RemoveBetweenAsync(requesterId, request.TargetUserId, cancellationToken);
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}