using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SocialHub.Identity.Services;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Identity;
 
public class CurrentUserServiceTests
{
    private static CurrentUserService CreateService(ClaimsPrincipal? user)
    {
        var accessor = new HttpContextAccessor();
 
        if (user is not null)
        {
            accessor.HttpContext = new DefaultHttpContext { User = user };
        }
 
        return new CurrentUserService(accessor);
    }
 
    [Fact]
    public void UserId_Should_ReturnNameIdentifierClaim_When_Authenticated()
    {
        var userId = Guid.NewGuid().ToString();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");
        var service = CreateService(new ClaimsPrincipal(identity));
 
        service.UserId.Should().Be(userId);
        service.IsAuthenticated.Should().BeTrue();
    }
 
    [Fact]
    public void Roles_Should_ReturnAllRoleClaims()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Moderator")
        }, "TestAuth");
        var service = CreateService(new ClaimsPrincipal(identity));
 
        service.Roles.Should().BeEquivalentTo(new[] { "Admin", "Moderator" });
    }
 
    [Fact]
    public void IsAuthenticated_Should_BeFalse_When_NoHttpContext()
    {
        var service = CreateService(null);
 
        service.IsAuthenticated.Should().BeFalse();
        service.UserId.Should().BeNull();
    }
}