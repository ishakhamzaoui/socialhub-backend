using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed record UpdateThemePreferenceCommand(ThemePreference Theme) : ICommand, IRequireAuthorization, ITransactionalRequest;