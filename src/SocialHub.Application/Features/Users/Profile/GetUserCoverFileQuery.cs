using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Features.Media;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed record GetUserCoverFileQuery(Guid UserId) : IQuery<MediaFileDto>, IRequireAuthorization;