using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed record GetMyProfileQuery : IQuery<UserProfileDto>, IRequireAuthorization;