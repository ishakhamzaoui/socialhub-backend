using Microsoft.Extensions.Options;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Identity.Options;
 
namespace SocialHub.Identity.Services;
 
public sealed class AppUrlProvider : IAppUrlProvider
{
    private readonly AppUrlOptions _options;
 
    public AppUrlProvider(IOptions<AppUrlOptions> options)
    {
        _options = options.Value;
    }
 
    public string BuildEmailConfirmationUrl(Guid userId, string token) =>
        _options.EmailConfirmationUrl
            .Replace("{userId}", userId.ToString())
            .Replace("{token}", Uri.EscapeDataString(token));
 
    public string BuildPasswordResetUrl(string email, string token) =>
        _options.PasswordResetUrl
            .Replace("{email}", Uri.EscapeDataString(email))
            .Replace("{token}", Uri.EscapeDataString(token));
}