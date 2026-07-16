using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed record UpdateProfileCommand(
    string DisplayName,
    string? Bio,
    string? Location,
    string? Website) : ICommand<UserProfileDto>, IRequireAuthorization, ITransactionalRequest;