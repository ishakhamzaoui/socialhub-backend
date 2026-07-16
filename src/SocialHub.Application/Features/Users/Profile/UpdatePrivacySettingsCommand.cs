using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>
/// Roadmap 5.4. A single visibility level covers the profile as a whole
/// plus avatar/cover — see ProfileVisibility's remarks for why this isn't
/// split per-field.
/// </summary>
public sealed record UpdatePrivacySettingsCommand(ProfileVisibility Visibility) : ICommand, IRequireAuthorization, ITransactionalRequest;