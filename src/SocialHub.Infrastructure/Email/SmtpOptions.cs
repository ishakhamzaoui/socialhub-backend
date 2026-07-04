namespace SocialHub.Infrastructure.Email;
 
/// <summary>
/// Generic SMTP relay settings — works with any provider that exposes a
/// standard SMTP endpoint (SendGrid, Mailgun, etc.), per project convention.
/// Host/Username/Password ship empty in appsettings.json on purpose; set the
/// real values via 'dotnet user-secrets' (dev) or environment variables
/// (prod) — never commit them (spec §24).
/// </summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
 
    public int Port { get; set; } = 587;
 
    public string Username { get; set; } = string.Empty;
 
    public string Password { get; set; } = string.Empty;
 
    public string FromEmail { get; set; } = "no-reply@socialhub.local";
 
    public string FromName { get; set; } = "SocialHub";
 
    public bool UseStartTls { get; set; } = true;
}