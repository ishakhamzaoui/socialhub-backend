using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class GetMyProfileQueryHandler : IQueryHandler<GetMyProfileQuery, UserProfileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
 
    public GetMyProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
    }
 
    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Result.Failure<UserProfileDto>(
                Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            // Shouldn't happen post-registration-fix (see RegisterCommandHandler)
            // for any account created from this point on. Existing accounts
            // predating this migration are backfilled by the seeder (dev
            // admin) — any other pre-Phase-5 account is a known gap.
            return Result.Failure<UserProfileDto>(
                Error.NotFound("Profile.NotFound", "No profile exists for this account yet."));
        }
 
        var followerCount = await _followRepository.GetFollowerCountAsync(userId, cancellationToken);
        var followingCount = await _followRepository.GetFollowingCountAsync(userId, cancellationToken);
 
        return Result.Success(UserProfileDto.From(profile, followerCount, followingCount, isFollowedByRequester: false, isOwnProfile: true));
    }
}