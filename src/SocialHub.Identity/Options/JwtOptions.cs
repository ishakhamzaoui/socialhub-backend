namespace SocialHub.Identity.Options;
 
/// <summary>
/// Binds to the existing "Jwt" configuration section (appsettings.json,
/// already scaffolded in Phase 0 for AuthExtensions.AddSocialHubAuthentication
/// on the validation side). This class is the issuing side's view of the
/// same keys — Issuer/Audience/Key must match exactly or issued tokens will
/// fail the validation parameters already configured there.
/// </summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Key { get; set; } = default!;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}