using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Media;
 
/// <summary>
/// Internal to the download/thumbnail endpoints (SocialHub.API.Controllers.MediaController)
/// — unlike MediaDto, this deliberately exposes the raw relative storage
/// path, since the controller needs it to open the file via IFileStorageService.
/// Never returned directly from a public API response.
/// </summary>
public sealed record GetMediaFileQuery(Guid MediaId, bool Thumbnail = false) : IQuery<MediaFileDto>, IRequireAuthorization;
 
public sealed record MediaFileDto(string RelativePath, string MimeType, string FileName);