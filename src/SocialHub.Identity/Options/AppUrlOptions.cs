namespace SocialHub.Identity.Options;
 
/// <summary>
/// URL templates for links embedded in emails. {userId}/{token}/{email}
/// placeholders are substituted by AppUrlProvider. Defaults point at a
/// placeholder frontend origin — update AppUrls:* in appsettings once a
/// real frontend exists.
/// </summary>
public sealed class AppUrlOptions
{
    public string EmailConfirmationUrl { get; set; } = "http://localhost:5173/confirm-email?userId={userId}&token={token}";
 
    public string PasswordResetUrl { get; set; } = "http://localhost:5173/reset-password?email={email}&token={token}";
}