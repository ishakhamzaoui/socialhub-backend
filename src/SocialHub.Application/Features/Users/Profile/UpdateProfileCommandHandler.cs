using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Result.Failure<UserProfileDto>(
                Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<UserProfileDto>(Error.NotFound("Profile.NotFound", "No profile exists for this account yet."));
        }
 
        profile.UpdateDetails(request.DisplayName, request.Bio, request.Location, request.Website);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var followerCount = await _followRepository.GetFollowerCountAsync(userId, cancellationToken);
        var followingCount = await _followRepository.GetFollowingCountAsync(userId, cancellationToken);
 
        return Result.Success(UserProfileDto.From(profile, followerCount, followingCount, isFollowedByRequester: false, isOwnProfile: true));
    }
}