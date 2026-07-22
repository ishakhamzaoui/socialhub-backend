using FluentAssertions;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class FeedRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public FeedRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetFollowingFeedAsync_Should_OnlyIncludePostsFromFollowedAuthors()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FeedRepository(context);
 
        var requesterId = Guid.NewGuid();
        var followedAuthorId = Guid.NewGuid();
        var unfollowedAuthorId = Guid.NewGuid();
 
        context.Set<Follow>().Add(Follow.Create(requesterId, followedAuthorId));
 
        var followedPost = Post.CreatePublished(followedAuthorId, "from someone I follow", PostVisibility.Public);
        var unfollowedPost = Post.CreatePublished(unfollowedAuthorId, "from someone I don't follow", PostVisibility.Public);
        context.Set<Post>().AddRange(followedPost, unfollowedPost);
        await context.SaveChangesAsync();
 
        var (entries, _) = await repository.GetFollowingFeedAsync(requesterId, cursor: null, pageSize: 20);
 
        entries.Select(e => e.PostId).Should().Contain(followedPost.Id);
        entries.Select(e => e.PostId).Should().NotContain(unfollowedPost.Id);
    }
 
    [Fact]
    public async Task GetChronologicalFeedAsync_Should_ExcludePosts_When_AuthorIsBlockedEitherDirection()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FeedRepository(context);
 
        var requesterId = Guid.NewGuid();
        var blockedAuthorId = Guid.NewGuid();
        var normalAuthorId = Guid.NewGuid();
 
        context.Set<UserBlock>().Add(UserBlock.Create(requesterId, blockedAuthorId));
 
        var blockedPost = Post.CreatePublished(blockedAuthorId, "from someone I blocked", PostVisibility.Public);
        var normalPost = Post.CreatePublished(normalAuthorId, "from anyone else", PostVisibility.Public);
        context.Set<Post>().AddRange(blockedPost, normalPost);
        await context.SaveChangesAsync();
 
        var (entries, _) = await repository.GetChronologicalFeedAsync(requesterId, cursor: null, pageSize: 20);
 
        entries.Select(e => e.PostId).Should().NotContain(blockedPost.Id);
        entries.Select(e => e.PostId).Should().Contain(normalPost.Id);
    }
 
    [Fact]
    public async Task GetChronologicalFeedAsync_Should_ExcludePrivateAndUnlistedPosts()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FeedRepository(context);
 
        var requesterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
 
        var privatePost = Post.CreatePublished(authorId, "private", PostVisibility.Private);
        var unlistedPost = Post.CreatePublished(authorId, "unlisted", PostVisibility.Unlisted);
        var publicPost = Post.CreatePublished(authorId, "public", PostVisibility.Public);
        context.Set<Post>().AddRange(privatePost, unlistedPost, publicPost);
        await context.SaveChangesAsync();
 
        var (entries, _) = await repository.GetChronologicalFeedAsync(requesterId, cursor: null, pageSize: 20);
 
        var postIds = entries.Select(e => e.PostId).ToList();
        postIds.Should().NotContain(privatePost.Id);
        postIds.Should().NotContain(unlistedPost.Id);
        postIds.Should().Contain(publicPost.Id);
    }
 
    [Fact]
    public async Task GetChronologicalFeedAsync_Should_AdvancePastFirstPage_When_CursorIsUsed()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FeedRepository(context);
 
        var requesterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
 
        // Three posts, deliberately created with distinct, increasing
        // CreatedAtUtc so page-1/page-2 ordering is unambiguous even though
        // all three are inserted in the same batch.
        var oldest = Post.CreatePublished(authorId, "oldest", PostVisibility.Public);
        await Task.Delay(10);
        var middle = Post.CreatePublished(authorId, "middle", PostVisibility.Public);
        await Task.Delay(10);
        var newest = Post.CreatePublished(authorId, "newest", PostVisibility.Public);
        context.Set<Post>().AddRange(oldest, middle, newest);
        await context.SaveChangesAsync();
 
        var (firstPage, hasMoreAfterFirst) = await repository.GetChronologicalFeedAsync(requesterId, cursor: null, pageSize: 1);
        firstPage.Should().HaveCount(1);
        firstPage[0].PostId.Should().Be(newest.Id);
        hasMoreAfterFirst.Should().BeTrue();
 
        var cursor = new SocialHub.Application.Common.Pagination.FeedCursor(firstPage[0].SortKey, firstPage[0].PostId);
        var (secondPage, _) = await repository.GetChronologicalFeedAsync(requesterId, cursor, pageSize: 1);
 
        secondPage.Should().HaveCount(1);
        secondPage[0].PostId.Should().Be(middle.Id);
    }
}