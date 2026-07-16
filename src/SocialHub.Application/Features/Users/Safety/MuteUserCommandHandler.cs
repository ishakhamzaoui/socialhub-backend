using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class MuteUserCommandHandler : ICommandHandler<MuteUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserMuteRepository _userMuteRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public MuteUserCommandHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IUserMuteRepository userMuteRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _userMuteRepository = userMuteRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(MuteUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        if (requesterId == request.TargetUserId)
        {
            return Result.Failure(Error.Validation("Mute.CannotMuteSelf", "You cannot mute yourself."));
        }
 
        var targetProfile = await _userProfileRepository.GetByUserIdAsync(request.TargetUserId, cancellationToken);
        if (targetProfile is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        if (await _userMuteRepository.IsMutedAsync(requesterId, request.TargetUserId, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Mute.AlreadyMuted", "You have already muted this user."));
        }
 
        var mute = Domain.Users.UserMute.Create(requesterId, request.TargetUserId);
        await _userMuteRepository.AddAsync(mute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}