using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users;
 
/// <summary>
/// Roadmap 5.1-5.9: the rich user profile the Phase 0-3 onboarding doc (§7)
/// deliberately deferred out of ApplicationUser. UserId is a bare Guid
/// (ApplicationUser.Id) — same pattern as RefreshToken.UserId and
/// MediaAsset.OwnerId: Domain cannot reference SocialHub.Identity.
///
/// AvatarMediaId/CoverMediaId are bare Guid? references to MediaAsset.Id —
/// not navigation properties. Domain.Users and Domain.Media are sibling
/// namespaces with no dependency between them; resolving these into an
/// actual file happens in the Application layer via IMediaAssetRepository.
///
/// Row lifecycle: created once, at registration time, by
/// Application.Features.Authentication.Register.RegisterCommandHandler
/// (a deliberate Phase 5 addition to that Phase 3 handler — see the Phase 5
/// context doc's "corrections" section for why: creating it eagerly avoids
/// making profile *read* paths side-effecting when looking up another
/// user's profile, e.g. for a followers list).
/// </summary>
public sealed class UserProfile : BaseEntity, IAggregateRoot
{
    private UserProfile()
    {
        // Reserved for EF Core materialization.
    }
 
    private UserProfile(Guid id, Guid userId, string displayName)
        : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        Visibility = ProfileVisibility.Public;
        Theme = ThemePreference.System;
        Language = "en";
        IsVerified = false;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid UserId { get; private set; }
 
    public string DisplayName { get; private set; } = default!;
 
    public string? Bio { get; private set; }
 
    public string? Location { get; private set; }
 
    public string? Website { get; private set; }
 
    /// <summary>MediaAsset.Id of the current avatar. Prior avatars are not deleted (kept as history per Phase 5 decision) but are no longer referenced from here once replaced.</summary>
    public Guid? AvatarMediaId { get; private set; }
 
    /// <summary>MediaAsset.Id of the current cover image. Same history-retention rule as AvatarMediaId.</summary>
    public Guid? CoverMediaId { get; private set; }
 
    public ProfileVisibility Visibility { get; private set; }
 
    public ThemePreference Theme { get; private set; }
 
    /// <summary>ISO 639-1 two-letter code (e.g. "en", "fr"). Validated at the Application layer (UpdateLanguagePreferenceCommandValidator).</summary>
    public string Language { get; private set; } = default!;
 
    /// <summary>Roadmap 5.9. Set only via the admin-only VerifyUserCommand (Permissions.Users.Manage) — not user-settable.</summary>
    public bool IsVerified { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public DateTime? UpdatedAtUtc { get; private set; }
 
    public static UserProfile CreateDefault(Guid userId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }
 
        return new UserProfile(Guid.NewGuid(), userId, displayName);
    }
 
    public void UpdateDetails(string displayName, string? bio, string? location, string? website)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }
 
        DisplayName = displayName;
        Bio = bio;
        Location = location;
        Website = website;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void UpdateVisibility(ProfileVisibility visibility)
    {
        Visibility = visibility;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void UpdateTheme(ThemePreference theme)
    {
        Theme = theme;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void UpdateLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language cannot be empty.", nameof(language));
        }
 
        Language = language.ToLowerInvariant();
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>Pass null to clear (e.g. never expected in practice, but keeps the setter symmetric).</summary>
    public void SetAvatar(Guid? mediaId)
    {
        AvatarMediaId = mediaId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void SetCover(Guid? mediaId)
    {
        CoverMediaId = mediaId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void SetVerified(bool isVerified)
    {
        IsVerified = isVerified;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}