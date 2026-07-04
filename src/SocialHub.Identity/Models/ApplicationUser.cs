using Microsoft.AspNetCore.Identity;
 
namespace SocialHub.Identity.Models;
 
/// <summary>
/// Customized ASP.NET Core Identity user. Guid keys to stay consistent with
/// the rest of the domain (SocialHub.Domain.Common.BaseEntity also keys on
/// Guid).
///
/// Deliberately stays a thin authentication record — the rich user *profile*
/// (bio, avatar, privacy settings, theme, etc. — spec §15.2) is a separate
/// Domain aggregate arriving in Phase 5 (User Management), linked back to
/// this record by Id. Do not add profile fields here.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
 
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
 
    public DateTime? LastLoginAtUtc { get; set; }
}