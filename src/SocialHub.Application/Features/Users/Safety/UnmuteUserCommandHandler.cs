using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class UnmuteUserCommandHandler : ICommandHandler<UnmuteUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserMuteRepository _userMuteRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UnmuteUserCommandHandler(
        ICurrentUserService currentUserService,
        IUserMuteRepository userMuteRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userMuteRepository = userMuteRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UnmuteUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var mute = await _userMuteRepository.GetAsync(requesterId, request.TargetUserId, cancellationToken);
        if (mute is null)
        {
            return Result.Failure(Error.NotFound("Mute.NotFound", "You have not muted this user."));
        }
 
        _userMuteRepository.Remove(mute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}