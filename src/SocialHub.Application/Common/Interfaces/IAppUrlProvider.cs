namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Builds the links embedded in verification/reset emails. Implemented in
/// SocialHub.Identity, reading configurable URL templates (AppUrls:* in
/// appsettings) so the Application layer never deals with configuration or
/// URL-encoding directly.
/// </summary>
public interface IAppUrlProvider
{
    string BuildEmailConfirmationUrl(Guid userId, string token);
 
    string BuildPasswordResetUrl(string email, string token);
}