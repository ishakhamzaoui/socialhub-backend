using FluentAssertions;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Domain.Tests.Users;
 
public class UserBlockTests
{
    [Fact]
    public void Create_Should_SetBlockerAndBlocked()
    {
        var blockerId = Guid.NewGuid();
        var blockedId = Guid.NewGuid();
 
        var block = UserBlock.Create(blockerId, blockedId);
 
        block.BlockerId.Should().Be(blockerId);
        block.BlockedId.Should().Be(blockedId);
    }
 
    [Fact]
    public void Create_Should_Throw_When_BlockingSelf()
    {
        var userId = Guid.NewGuid();
 
        var act = () => UserBlock.Create(userId, userId);
 
        act.Should().Throw<ArgumentException>();
    }
}