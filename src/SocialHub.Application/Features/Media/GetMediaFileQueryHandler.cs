using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Media;
 
public sealed class GetMediaFileQueryHandler : IQueryHandler<GetMediaFileQuery, MediaFileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaAssetRepository _repository;
 
    public GetMediaFileQueryHandler(ICurrentUserService currentUserService, IMediaAssetRepository repository)
    {
        _currentUserService = currentUserService;
        _repository = repository;
    }
 
    public async Task<Result<MediaFileDto>> Handle(GetMediaFileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var ownerId))
        {
            return Result.Failure<MediaFileDto>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var asset = await _repository.GetByIdForOwnerAsync(request.MediaId, ownerId, cancellationToken);
        if (asset is null)
        {
            return Result.Failure<MediaFileDto>(Error.NotFound("Media.NotFound", "Media not found."));
        }
 
        if (request.Thumbnail)
        {
            if (asset.ThumbnailStoragePath is null)
            {
                return Result.Failure<MediaFileDto>(Error.NotFound("Media.ThumbnailNotFound", "This media has no thumbnail."));
            }
 
            return Result.Success(new MediaFileDto(asset.ThumbnailStoragePath, "image/jpeg", $"thumb-{asset.OriginalFileName}"));
        }
 
        return Result.Success(new MediaFileDto(asset.StoragePath, asset.MimeType, asset.OriginalFileName));
    }
}