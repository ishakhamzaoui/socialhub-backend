using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Features.Media;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>
/// Reuses Media.MediaFileDto's shape (RelativePath/MimeType/FileName) rather
/// than inventing a parallel DTO — same reasoning as GetMediaFileQuery:
/// internal to the avatar-download controller action, never returned
/// directly from a public API response.
/// </summary>
public sealed record GetUserAvatarFileQuery(Guid UserId) : IQuery<MediaFileDto>, IRequireAuthorization;