using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Media;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class GetUserCoverFileQueryHandler : IQueryHandler<GetUserCoverFileQuery, MediaFileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
 
    public GetUserCoverFileQueryHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository,
        IMediaAssetRepository mediaAssetRepository)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
        _mediaAssetRepository = mediaAssetRepository;
    }
 
    public async Task<Result<MediaFileDto>> Handle(GetUserCoverFileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<MediaFileDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var profile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null || profile.CoverMediaId is null)
        {
            return Result.Failure<MediaFileDto>(Error.NotFound("Profile.CoverNotSet", "This user has no cover image."));
        }
 
        var access = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, profile, requesterId, cancellationToken);
        switch (access)
        {
            case ProfileAccessResult.Blocked:
                return Result.Failure<MediaFileDto>(Error.NotFound("Profile.NotFound", "This user could not be found."));
            case ProfileAccessResult.Denied:
                return Result.Failure<MediaFileDto>(Error.Forbidden("Profile.Private", "This profile is not visible to you."));
        }
 
        var asset = await _mediaAssetRepository.GetByIdAsync(profile.CoverMediaId.Value, cancellationToken);
        if (asset is null)
        {
            return Result.Failure<MediaFileDto>(Error.NotFound("Profile.CoverNotSet", "This user has no cover image."));
        }
 
        return Result.Success(new MediaFileDto(asset.StoragePath, asset.MimeType, asset.OriginalFileName));
    }
}