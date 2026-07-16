using FluentAssertions;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Domain.Tests.Users;
 
public class UserMuteTests
{
    [Fact]
    public void Create_Should_SetMuterAndMuted()
    {
        var muterId = Guid.NewGuid();
        var mutedId = Guid.NewGuid();
 
        var mute = UserMute.Create(muterId, mutedId);
 
        mute.MuterId.Should().Be(muterId);
        mute.MutedId.Should().Be(mutedId);
    }
 
    [Fact]
    public void Create_Should_Throw_When_MutingSelf()
    {
        var userId = Guid.NewGuid();
 
        var act = () => UserMute.Create(userId, userId);
 
        act.Should().Throw<ArgumentException>();
    }
}