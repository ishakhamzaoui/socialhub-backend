using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Media;
 
public sealed record GetMediaQuery(Guid MediaId) : IQuery<MediaDto>, IRequireAuthorization;