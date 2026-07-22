using FluentAssertions;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using Xunit;
 
namespace SocialHub.Domain.Tests.Posts;
 
public class PostReactionTests
{
    [Fact]
    public void Create_Should_SetPostUserAndType()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
 
        var reaction = PostReaction.Create(postId, userId, ReactionType.Love);
 
        reaction.PostId.Should().Be(postId);
        reaction.UserId.Should().Be(userId);
        reaction.Type.Should().Be(ReactionType.Love);
        reaction.UpdatedAtUtc.Should().BeNull();
    }
 
    [Fact]
    public void ChangeType_Should_UpdateType_And_SetUpdatedAtUtc()
    {
        var reaction = PostReaction.Create(Guid.NewGuid(), Guid.NewGuid(), ReactionType.Like);
 
        reaction.ChangeType(ReactionType.Angry);
 
        reaction.Type.Should().Be(ReactionType.Angry);
        reaction.UpdatedAtUtc.Should().NotBeNull();
    }
}