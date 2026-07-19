using FluentAssertions;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Comments.Events;
using Xunit;
 
namespace SocialHub.Domain.Tests.Comments;
 
public class CommentTests
{
    [Fact]
    public void Create_Should_RaiseCommentAddedEvent_And_SetTopLevel_When_NoParent()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
 
        var comment = Comment.Create(postId, authorId, "hello");
 
        comment.PostId.Should().Be(postId);
        comment.AuthorId.Should().Be(authorId);
        comment.ParentCommentId.Should().BeNull();
        comment.Content.Should().Be("hello");
        comment.IsDeleted.Should().BeFalse();
        comment.IsPinned.Should().BeFalse();
        comment.DomainEvents.Should().ContainSingle(e => e is CommentAddedEvent);
    }
 
    [Fact]
    public void Create_Should_SetParentCommentId_ForReply()
    {
        var parentId = Guid.NewGuid();
 
        var reply = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "a reply", parentId);
 
        reply.ParentCommentId.Should().Be(parentId);
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_ContentEmpty(string? content)
    {
        var act = () => Comment.Create(Guid.NewGuid(), Guid.NewGuid(), content!);
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void UpdateContent_Should_SetContent_And_UpdatedAtUtc()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "original");
 
        comment.UpdateContent("edited");
 
        comment.Content.Should().Be("edited");
        comment.UpdatedAtUtc.Should().NotBeNull();
    }
 
    [Fact]
    public void UpdateContent_Should_Throw_When_Deleted()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "original");
        comment.MarkDeleted();
 
        var act = () => comment.UpdateContent("edited");
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void UpdateContent_Should_Throw_When_ContentEmpty()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "original");
 
        var act = () => comment.UpdateContent("   ");
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void Pin_Should_SetIsPinned()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
 
        comment.Pin();
 
        comment.IsPinned.Should().BeTrue();
    }
 
    [Fact]
    public void Pin_Should_Throw_When_Deleted()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        comment.MarkDeleted();
 
        var act = () => comment.Pin();
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void Unpin_Should_ClearIsPinned()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        comment.Pin();
 
        comment.Unpin();
 
        comment.IsPinned.Should().BeFalse();
    }
 
    [Fact]
    public void AddMention_Should_SkipSelfMention()
    {
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), authorId, "hello");
 
        comment.AddMention(authorId);
 
        comment.Mentions.Should().BeEmpty();
    }
 
    [Fact]
    public void AddMention_Should_Deduplicate()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        var mentionedUserId = Guid.NewGuid();
 
        comment.AddMention(mentionedUserId);
        comment.AddMention(mentionedUserId);
 
        comment.Mentions.Should().ContainSingle();
    }
 
    [Fact]
    public void MarkDeleted_Should_TombstoneContent_And_ClearIsPinned_And_RaiseCommentDeletedEvent()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        comment.Pin();
        comment.ClearDomainEvents();
 
        comment.MarkDeleted();
 
        comment.IsDeleted.Should().BeTrue();
        comment.Content.Should().BeNull();
        comment.IsPinned.Should().BeFalse();
        comment.DeletedAtUtc.Should().NotBeNull();
        comment.DomainEvents.Should().ContainSingle(e => e is CommentDeletedEvent);
    }
 
    [Fact]
    public void MarkDeleted_Should_Throw_When_AlreadyDeleted()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        comment.MarkDeleted();
 
        var act = () => comment.MarkDeleted();
 
        act.Should().Throw<InvalidOperationException>();
    }
}