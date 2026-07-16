using FluentAssertions;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Domain.Tests.Users;
 
public class UserProfileTests
{
    [Fact]
    public void CreateDefault_Should_SetSensibleDefaults()
    {
        var userId = Guid.NewGuid();
 
        var profile = UserProfile.CreateDefault(userId, "alice");
 
        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be("alice");
        profile.Visibility.Should().Be(ProfileVisibility.Public);
        profile.Theme.Should().Be(ThemePreference.System);
        profile.Language.Should().Be("en");
        profile.IsVerified.Should().BeFalse();
        profile.AvatarMediaId.Should().BeNull();
        profile.CoverMediaId.Should().BeNull();
    }
 
    [Fact]
    public void CreateDefault_Should_Throw_When_DisplayNameIsEmpty()
    {
        var act = () => UserProfile.CreateDefault(Guid.NewGuid(), "  ");
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void UpdateDetails_Should_UpdateFieldsAndTimestamp()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
 
        profile.UpdateDetails("Alice Smith", "bio", "NYC", "https://example.com");
 
        profile.DisplayName.Should().Be("Alice Smith");
        profile.Bio.Should().Be("bio");
        profile.Location.Should().Be("NYC");
        profile.Website.Should().Be("https://example.com");
        profile.UpdatedAtUtc.Should().NotBeNull();
    }
 
    [Fact]
    public void SetAvatar_Should_RepointWithoutClearingCover()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
        var coverId = Guid.NewGuid();
        profile.SetCover(coverId);
 
        var avatarId = Guid.NewGuid();
        profile.SetAvatar(avatarId);
 
        profile.AvatarMediaId.Should().Be(avatarId);
        profile.CoverMediaId.Should().Be(coverId);
    }
 
    [Fact]
    public void SetVerified_Should_UpdateIsVerified()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
 
        profile.SetVerified(true);
 
        profile.IsVerified.Should().BeTrue();
    }
 
    [Fact]
    public void UpdateLanguage_Should_NormalizeToLowercase()
    {
        var profile = UserProfile.CreateDefault(Guid.NewGuid(), "alice");
 
        profile.UpdateLanguage("FR");
 
        profile.Language.Should().Be("fr");
    }
}