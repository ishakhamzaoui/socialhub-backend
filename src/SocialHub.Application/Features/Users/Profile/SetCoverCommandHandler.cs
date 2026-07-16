using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Media;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class SetCoverCommandHandler : ICommandHandler<SetCoverCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public SetCoverCommandHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IMediaAssetRepository mediaAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(SetCoverCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var asset = await _mediaAssetRepository.GetByIdForOwnerAsync(request.MediaAssetId, userId, cancellationToken);
        if (asset is null)
        {
            return Result.Failure(Error.NotFound("Media.NotFound", "Media not found."));
        }
 
        if (asset.Kind != MediaKind.Image)
        {
            return Result.Failure(Error.Validation("Media.InvalidKind", "A cover image must be an image."));
        }
 
        if (asset.Category != MediaCategory.User)
        {
            return Result.Failure(Error.Validation("Media.InvalidCategory", "Media must be uploaded under the User category to be used as a cover image."));
        }
 
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(Error.NotFound("Profile.NotFound", "No profile exists for this account yet."));
        }
 
        profile.SetCover(asset.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}