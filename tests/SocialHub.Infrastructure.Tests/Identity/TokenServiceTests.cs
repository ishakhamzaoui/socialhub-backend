using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SocialHub.Identity.Options;
using SocialHub.Identity.Token;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Identity;
 
public class TokenServiceTests
{
    private static TokenService CreateService(string key = "this-is-a-test-signing-key-that-is-long-enough-1234567890") =>
        new(Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = key,
            AccessTokenMinutes = 15,
            RefreshTokenDays = 30
        }));
 
    [Fact]
    public void GenerateAccessToken_Should_ProduceTokenWithExpectedClaims()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
 
        var token = service.GenerateAccessToken(userId, "user@example.com", new[] { "Admin" }, new[] { "Permissions.Users.Manage" });
 
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "Permissions.Users.Manage");
    }
 
    [Fact]
    public void GenerateAccessToken_Should_Throw_When_KeyIsNotConfigured()
    {
        var service = CreateService(key: string.Empty);
 
        var act = () => service.GenerateAccessToken(Guid.NewGuid(), "user@example.com", new[] { "User" });
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void GenerateRefreshToken_Should_ProduceTokenWhoseHashMatchesHashToken()
    {
        var service = CreateService();
 
        var (rawToken, tokenHash) = service.GenerateRefreshToken();
 
        service.HashToken(rawToken).Should().Be(tokenHash);
    }
 
    [Fact]
    public void GenerateRefreshToken_Should_NeverProduceTheSameTokenTwice()
    {
        var service = CreateService();
 
        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();
 
        first.RawToken.Should().NotBe(second.RawToken);
    }
}