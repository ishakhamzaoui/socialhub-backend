using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users;
 
/// <summary>
/// Opaque, single-use refresh token. The raw token value is never persisted —
/// only its SHA-256 hash (SocialHub.Identity.Token.TokenService does the
/// hashing) — so a leaked database backup does not hand out usable refresh
/// tokens.
///
/// Rotation model: redeeming a token revokes it and stamps
/// ReplacedByTokenHash in the same operation. If an already-revoked token is
/// ever presented again, RefreshTokenCommandHandler treats that as a signal
/// of possible theft and revokes the entire token family for that user.
///
/// Lives in Domain (not Identity): the Application-layer auth command
/// handlers need to read/write this entity via the existing generic
/// IRepository&lt;TEntity,TId&gt;, and Application cannot reference
/// SocialHub.Identity (would be circular). Unlike ApplicationUser/
/// ApplicationRole, RefreshToken has no dependency on ASP.NET Core Identity
/// base classes, so it belongs here per spec §6.
///
/// Mirrors SocialHub.Domain.Shared.Hashtag's exact construction pattern
/// (private ctor + static Create factory over BaseEntity's protected
/// Guid-id constructor).
/// </summary>
public sealed class RefreshToken : BaseEntity, IAggregateRoot
{
    private RefreshToken()
    {
        // Reserved for EF Core materialization.
    }
 
    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }
 
    public Guid UserId { get; private set; }
 
    public string TokenHash { get; private set; } = default!;
 
    public DateTime ExpiresAtUtc { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public string? CreatedByIp { get; private set; }
 
    public DateTime? RevokedAtUtc { get; private set; }
 
    public string? RevokedByIp { get; private set; }
 
    public string? ReplacedByTokenHash { get; private set; }
 
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
 
    public bool IsRevoked => RevokedAtUtc is not null;
 
    public bool IsActive => !IsRevoked && !IsExpired;
 
    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));
        }
 
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc, createdByIp);
    }
 
    public void Revoke(string? revokedByIp, string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}