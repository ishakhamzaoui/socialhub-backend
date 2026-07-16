using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>
/// Attaches an already-uploaded MediaAsset (MediaCategory.User, MediaKind.Image)
/// to the caller's profile as their avatar. MediaAssetId must already belong
/// to the caller — see SetAvatarCommandHandler's ownership check via
/// IMediaAssetRepository.GetByIdForOwnerAsync (the same owner-scoped method
/// Phase 4's GetMediaFileQueryHandler uses).
/// </summary>
public sealed record SetAvatarCommand(Guid MediaAssetId) : ICommand, IRequireAuthorization, ITransactionalRequest;