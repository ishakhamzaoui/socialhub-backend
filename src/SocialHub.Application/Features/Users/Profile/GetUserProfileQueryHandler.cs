using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetUserProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
    }
 
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<UserProfileDto>(
                Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var profile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<UserProfileDto>(Error.NotFound("Profile.NotFound", "This user could not be found."));
        }
 
        var access = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, requesterId, cancellationToken);
 
        switch (access)
        {
            case ProfileAccessResult.Blocked:
                // Deliberately NotFound, not Forbidden — a block's existence
                // is never revealed to either party through this endpoint.
                return Result.Failure<UserProfileDto>(Error.NotFound("Profile.NotFound", "This user could not be found."));
            case ProfileAccessResult.Denied:
                return Result.Failure<UserProfileDto>(Error.Forbidden("Profile.Private", "This profile is not visible to you."));
        }
 
        var followerCount = await _followRepository.GetFollowerCountAsync(profile.UserId, cancellationToken);
        var followingCount = await _followRepository.GetFollowingCountAsync(profile.UserId, cancellationToken);
        var isOwnProfile = requesterId == profile.UserId;
        var isFollowedByRequester = !isOwnProfile && await _followRepository.ExistsAsync(requesterId, profile.UserId, cancellationToken);
 
        return Result.Success(UserProfileDto.From(profile, followerCount, followingCount, isFollowedByRequester, isOwnProfile));
    }
}