using FluentAssertions;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Shared;
using Xunit;
 
namespace SocialHub.Domain.Tests.Comments;
 
public class CommentReactionTests
{
    [Fact]
    public void Create_Should_SetCommentUserAndType()
    {
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
 
        var reaction = CommentReaction.Create(commentId, userId, ReactionType.Love);
 
        reaction.CommentId.Should().Be(commentId);
        reaction.UserId.Should().Be(userId);
        reaction.Type.Should().Be(ReactionType.Love);
        reaction.UpdatedAtUtc.Should().BeNull();
    }
 
    [Fact]
    public void ChangeType_Should_UpdateType_And_SetUpdatedAtUtc()
    {
        var reaction = CommentReaction.Create(Guid.NewGuid(), Guid.NewGuid(), ReactionType.Like);
 
        reaction.ChangeType(ReactionType.Angry);
 
        reaction.Type.Should().Be(ReactionType.Angry);
        reaction.UpdatedAtUtc.Should().NotBeNull();
    }
}