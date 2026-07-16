using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class VerifyUserCommandHandler : ICommandHandler<VerifyUserCommand>
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public VerifyUserCommandHandler(IUserProfileRepository userProfileRepository, IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(request.TargetUserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        profile.SetVerified(request.IsVerified);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}