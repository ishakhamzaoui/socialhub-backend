using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Media;
 
public sealed class GetMediaQueryHandler : IQueryHandler<GetMediaQuery, MediaDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaAssetRepository _repository;
 
    public GetMediaQueryHandler(ICurrentUserService currentUserService, IMediaAssetRepository repository)
    {
        _currentUserService = currentUserService;
        _repository = repository;
    }
 
    public async Task<Result<MediaDto>> Handle(GetMediaQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var ownerId))
        {
            return Result.Failure<MediaDto>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        // Interim rule for Phase 4: only the owner can view their own media.
        // No visibility/privacy model exists yet (Phase 5's PrivacySettings,
        // Phase 6's post visibility rules) — this will need to loosen once
        // those land, e.g. so a public avatar can be fetched by other users.
        // Flagged in the Phase 4 context doc as a follow-up for Phase 5/6.
        var asset = await _repository.GetByIdForOwnerAsync(request.MediaId, ownerId, cancellationToken);
        if (asset is null)
        {
            return Result.Failure<MediaDto>(Error.NotFound("Media.NotFound", "Media not found."));
        }
 
        return Result.Success(MediaDto.From(asset));
    }
}