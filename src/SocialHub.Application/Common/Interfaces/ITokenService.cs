namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Issues JWT access tokens and opaque refresh tokens. Implemented in
/// SocialHub.Identity (TokenService) and registered after AddApplication()
/// in Program.cs, following the same override pattern already established
/// for IUnitOfWork (Phase 2) and ICurrentUserService (this phase).
///
/// Deliberately takes primitive/BCL types only (Guid, string,
/// IEnumerable&lt;string&gt;) rather than ApplicationUser, so the Application
/// layer can depend on this interface without referencing SocialHub.Identity.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed, short-lived JWT access token.</summary>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string>? permissions = null);
 
    /// <summary>
    /// Generates a new opaque refresh token. RawToken is returned to the
    /// caller (client) exactly once; only TokenHash is ever persisted
    /// (see RefreshToken entity).
    /// </summary>
    (string RawToken, string TokenHash) GenerateRefreshToken();
 
    /// <summary>Hashes a raw refresh token for lookup/comparison against stored TokenHash values.</summary>
    string HashToken(string rawToken);
 
    /// <summary>How long a newly issued refresh token should live (Jwt:RefreshTokenDays).</summary>
    TimeSpan RefreshTokenLifetime { get; }
}