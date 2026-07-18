using FluentAssertions;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Posts.Events;
using Xunit;
 
namespace SocialHub.Domain.Tests.Posts;
 
public class PostTests
{
    [Fact]
    public void CreateDraft_Should_RaisePostCreatedEvent_And_SetStatusDraft()
    {
        var authorId = Guid.NewGuid();
 
        var post = Post.CreateDraft(authorId, "hello", PostVisibility.Public);
 
        post.Status.Should().Be(PostStatus.Draft);
        post.PublishedAtUtc.Should().BeNull();
        post.DomainEvents.Should().ContainSingle(e => e is PostCreatedEvent);
    }
 
    [Fact]
    public void CreatePublished_Should_SetPublishedAtUtc()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
 
        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAtUtc.Should().NotBeNull();
    }
 
    [Fact]
    public void CreateScheduled_Should_Throw_When_ScheduledForUtc_NotInFuture()
    {
        var act = () => Post.CreateScheduled(Guid.NewGuid(), "hello", PostVisibility.Public, DateTime.UtcNow.AddMinutes(-1));
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void CreateScheduled_Should_SetStatusScheduled()
    {
        var scheduledFor = DateTime.UtcNow.AddHours(1);
 
        var post = Post.CreateScheduled(Guid.NewGuid(), "hello", PostVisibility.Public, scheduledFor);
 
        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledForUtc.Should().Be(scheduledFor);
        post.PublishedAtUtc.Should().BeNull();
    }
 
    [Fact]
    public void Create_Should_Throw_When_QuoteWithoutOriginalPostId()
    {
        var act = () => Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public, PostType.Quote, originalPostId: null);
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void Create_Should_Throw_When_OriginalTypeHasOriginalPostId()
    {
        var act = () => Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public, PostType.Original, originalPostId: Guid.NewGuid());
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void Publish_Should_Throw_When_AlreadyPublished()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
 
        var act = () => post.Publish();
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void Publish_Should_TransitionFromScheduled_And_ClearScheduledForUtc()
    {
        var post = Post.CreateScheduled(Guid.NewGuid(), "hello", PostVisibility.Public, DateTime.UtcNow.AddHours(1));
 
        post.Publish();
 
        post.Status.Should().Be(PostStatus.Published);
        post.ScheduledForUtc.Should().BeNull();
        post.PublishedAtUtc.Should().NotBeNull();
    }
 
    [Fact]
    public void Schedule_Should_Throw_When_NotInFuture()
    {
        var post = Post.CreateDraft(Guid.NewGuid(), "hello", PostVisibility.Public);
 
        var act = () => post.Schedule(DateTime.UtcNow.AddMinutes(-1));
 
        act.Should().Throw<ArgumentException>();
    }
 
    [Fact]
    public void Archive_Should_SetStatusArchived_And_ClearIsPinned()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        post.Pin();
 
        post.Archive();
 
        post.Status.Should().Be(PostStatus.Archived);
        post.IsPinned.Should().BeFalse();
    }
 
    [Fact]
    public void Archive_Should_Throw_When_AlreadyArchived()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        post.Archive();
 
        var act = () => post.Archive();
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void Pin_Should_Throw_When_NotPublished()
    {
        var post = Post.CreateDraft(Guid.NewGuid(), "hello", PostVisibility.Public);
 
        var act = () => post.Pin();
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void UpdateContent_Should_Throw_When_Archived()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        post.Archive();
 
        var act = () => post.UpdateContent("new content");
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void AttachMedia_Should_AddToMediaCollection_InGivenOrder()
    {
        var post = Post.CreateDraft(Guid.NewGuid(), null, PostVisibility.Public);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
 
        post.AttachMedia(first, order: 0);
        post.AttachMedia(second, order: 1);
 
        post.Media.Should().HaveCount(2);
        post.Media.Should().Contain(m => m.MediaAssetId == first && m.Order == 0);
        post.Media.Should().Contain(m => m.MediaAssetId == second && m.Order == 1);
    }
 
    [Fact]
    public void AddHashtag_Should_Deduplicate()
    {
        var post = Post.CreateDraft(Guid.NewGuid(), "hello", PostVisibility.Public);
        var hashtagId = Guid.NewGuid();
 
        post.AddHashtag(hashtagId);
        post.AddHashtag(hashtagId);
 
        post.Hashtags.Should().ContainSingle();
    }
 
    [Fact]
    public void AddMention_Should_SkipSelfMention()
    {
        var authorId = Guid.NewGuid();
        var post = Post.CreateDraft(authorId, "hello", PostVisibility.Public);
 
        post.AddMention(authorId);
 
        post.Mentions.Should().BeEmpty();
    }
 
    [Fact]
    public void AddMention_Should_Deduplicate()
    {
        var post = Post.CreateDraft(Guid.NewGuid(), "hello", PostVisibility.Public);
        var mentionedUserId = Guid.NewGuid();
 
        post.AddMention(mentionedUserId);
        post.AddMention(mentionedUserId);
 
        post.Mentions.Should().ContainSingle();
    }
 
    [Fact]
    public void MarkDeleted_Should_RaisePostDeletedEvent()
    {
        var authorId = Guid.NewGuid();
        var post = Post.CreatePublished(authorId, "hello", PostVisibility.Public);
        post.ClearDomainEvents();
 
        post.MarkDeleted();
 
        post.DomainEvents.Should().ContainSingle(e => e is PostDeletedEvent);
    }
}