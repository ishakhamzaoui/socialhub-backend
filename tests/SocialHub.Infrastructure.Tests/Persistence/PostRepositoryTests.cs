using FluentAssertions;
using SocialHub.Domain.Posts;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class PostRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public PostRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetByIdWithDetailsAsync_Should_LoadMediaHashtagsAndMentions()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepository(context);
 
        var post = Post.CreatePublished(Guid.NewGuid(), "hello #dotnet", SocialHub.Domain.Posts.PostVisibility.Public);
        var mediaAssetId = Guid.NewGuid();
        var hashtagId = Guid.NewGuid();
        var mentionedUserId = Guid.NewGuid();
        post.AttachMedia(mediaAssetId, order: 0);
        post.AddHashtag(hashtagId);
        post.AddMention(mentionedUserId);
 
        await repository.AddAsync(post);
        await context.SaveChangesAsync();
 
        // Fresh context, so this genuinely re-reads from the database rather
        // than returning the already-tracked in-memory instance.
        await using var freshContext = _fixture.CreateContext();
        var freshRepository = new PostRepository(freshContext);
        var reloaded = await freshRepository.GetByIdWithDetailsAsync(post.Id);
 
        reloaded.Should().NotBeNull();
        reloaded!.Media.Should().ContainSingle(m => m.MediaAssetId == mediaAssetId && m.Order == 0);
        reloaded.Hashtags.Should().ContainSingle(h => h.HashtagId == hashtagId);
        reloaded.Mentions.Should().ContainSingle(m => m.MentionedUserId == mentionedUserId);
    }
 
    [Fact]
    public async Task GetByIdForAuthorAsync_Should_ReturnNull_When_DifferentAuthor()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepository(context);
 
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", SocialHub.Domain.Posts.PostVisibility.Public);
        await repository.AddAsync(post);
        await context.SaveChangesAsync();
 
        var result = await repository.GetByIdForAuthorAsync(post.Id, Guid.NewGuid());
 
        result.Should().BeNull();
    }
 
    [Fact]
    public async Task GetPinnedPostAsync_Should_ReturnTheAuthorsPinnedPost()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepository(context);
 
        var authorId = Guid.NewGuid();
        var pinned = Post.CreatePublished(authorId, "pinned", SocialHub.Domain.Posts.PostVisibility.Public);
        pinned.Pin();
        var notPinned = Post.CreatePublished(authorId, "not pinned", SocialHub.Domain.Posts.PostVisibility.Public);
        await repository.AddAsync(pinned);
        await repository.AddAsync(notPinned);
        await context.SaveChangesAsync();
 
        var result = await repository.GetPinnedPostAsync(authorId);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(pinned.Id);
    }
 
    [Fact]
    public async Task GetByAuthorAsync_Should_FilterByStatus_And_ReturnCorrectTotal()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepository(context);
 
        var authorId = Guid.NewGuid();
        var draft = Post.CreateDraft(authorId, "draft", SocialHub.Domain.Posts.PostVisibility.Public);
        var published = Post.CreatePublished(authorId, "published", SocialHub.Domain.Posts.PostVisibility.Public);
        await repository.AddAsync(draft);
        await repository.AddAsync(published);
        await context.SaveChangesAsync();
 
        var (posts, total) = await repository.GetByAuthorAsync(authorId, PostStatus.Draft, page: 1, pageSize: 20);
 
        total.Should().Be(1);
        posts.Should().ContainSingle().Which.Id.Should().Be(draft.Id);
    }
 
    [Fact]
    public async Task GetDuePostsForPublishingAsync_Should_ReturnOnlyScheduledPostsPastDue()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepository(context);
 
        var due = Post.CreateScheduled(Guid.NewGuid(), "due", SocialHub.Domain.Posts.PostVisibility.Public, DateTime.UtcNow.AddMinutes(1));
        var notDue = Post.CreateScheduled(Guid.NewGuid(), "not due", SocialHub.Domain.Posts.PostVisibility.Public, DateTime.UtcNow.AddDays(1));
        await repository.AddAsync(due);
        await repository.AddAsync(notDue);
        await context.SaveChangesAsync();
 
        // asOfUtc is set just past "due"'s ScheduledForUtc but before "notDue"'s.
        var result = await repository.GetDuePostsForPublishingAsync(DateTime.UtcNow.AddMinutes(2));
 
        result.Should().ContainSingle(p => p.Id == due.Id);
        result.Should().NotContain(p => p.Id == notDue.Id);
    }
}