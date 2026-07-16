using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>Language is an ISO 639-1 two-letter code (e.g. "en", "fr") — validated, not an enum, since the set of supported languages isn't fixed by the Domain.</summary>
public sealed record UpdateLanguagePreferenceCommand(string Language) : ICommand, IRequireAuthorization, ITransactionalRequest;