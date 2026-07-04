using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Identity.Options;
 
namespace SocialHub.Identity.Token;
 
public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;
 
    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
 
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_options.RefreshTokenDays);
 
    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string>? permissions = null)
    {
        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException(
                "Jwt:Key is not configured. Set it via 'dotnet user-secrets set \"Jwt:Key\" \"...\"' " +
                "in Development, or an environment variable in Production (spec §24). " +
                "Script 12 provisions a dev-only value automatically if one wasn't already present.");
        }
 
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
 
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
 
        if (permissions is not null)
        {
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
        }
 
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
 
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);
 
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
 
    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (rawToken, HashToken(rawToken));
    }
 
    public string HashToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }
}