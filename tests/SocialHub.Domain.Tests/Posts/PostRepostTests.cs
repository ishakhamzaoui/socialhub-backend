using FluentAssertions;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Domain.Tests.Posts;
 
public class PostRepostTests
{
    [Fact]
    public void Create_Should_SetProperties()
    {
        var userId = Guid.NewGuid();
        var originalPostId = Guid.NewGuid();
 
        var repost = PostRepost.Create(userId, originalPostId);
 
        repost.UserId.Should().Be(userId);
        repost.OriginalPostId.Should().Be(originalPostId);
        repost.Id.Should().NotBeEmpty();
    }
}