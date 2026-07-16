using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>Public-view lookup of another user's profile. Visibility resolved via ProfileAccessPolicy.</summary>
public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileDto>, IRequireAuthorization;