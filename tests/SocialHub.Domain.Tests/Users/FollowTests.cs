using FluentAssertions;
using SocialHub.Domain.Users;
using SocialHub.Domain.Users.Events;
using Xunit;
 
namespace SocialHub.Domain.Tests.Users;
 
public class FollowTests
{
    [Fact]
    public void Create_Should_RaiseUserFollowedEvent()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
 
        var follow = Follow.Create(followerId, followingId);
 
        follow.FollowerId.Should().Be(followerId);
        follow.FollowingId.Should().Be(followingId);
        follow.DomainEvents.Should().ContainSingle(e => e is UserFollowedEvent);
    }
 
    [Fact]
    public void Create_Should_Throw_When_FollowingSelf()
    {
        var userId = Guid.NewGuid();
 
        var act = () => Follow.Create(userId, userId);
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void MarkUnfollowed_Should_RaiseUserUnfollowedEvent()
    {
        var follow = Follow.Create(Guid.NewGuid(), Guid.NewGuid());
        follow.ClearDomainEvents();
 
        follow.MarkUnfollowed();
 
        follow.DomainEvents.Should().ContainSingle(e => e is UserUnfollowedEvent);
    }
}