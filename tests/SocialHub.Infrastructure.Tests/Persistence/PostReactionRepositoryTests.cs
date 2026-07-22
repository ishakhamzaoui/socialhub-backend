using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class PostReactionRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public PostReactionRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetAsync_Should_ReturnTheUsersReactionOnThatPost()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostReactionRepository(context);
 
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reaction = PostReaction.Create(postId, userId, ReactionType.Laugh);
        await repository.AddAsync(reaction);
        await context.SaveChangesAsync();
 
        var result = await repository.GetAsync(postId, userId);
 
        result.Should().NotBeNull();
        result!.Type.Should().Be(ReactionType.Laugh);
    }
 
    [Fact]
    public async Task GetCountsByTypeAsync_Should_AggregateCorrectlyAcrossUsers()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostReactionRepository(context);
 
        var postId = Guid.NewGuid();
        await repository.AddAsync(PostReaction.Create(postId, Guid.NewGuid(), ReactionType.Like));
        await repository.AddAsync(PostReaction.Create(postId, Guid.NewGuid(), ReactionType.Like));
        await repository.AddAsync(PostReaction.Create(postId, Guid.NewGuid(), ReactionType.Sad));
        await context.SaveChangesAsync();
 
        var counts = await repository.GetCountsByTypeAsync(postId);
 
        counts[ReactionType.Like].Should().Be(2);
        counts[ReactionType.Sad].Should().Be(1);
        counts.ContainsKey(ReactionType.Angry).Should().BeFalse();
    }
 
    [Fact]
    public async Task UniqueIndex_Should_RejectASecondReaction_ForSameUserAndPost()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostReactionRepository(context);
 
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await repository.AddAsync(PostReaction.Create(postId, userId, ReactionType.Like));
        await context.SaveChangesAsync();
 
        // Deliberately bypassing the application-layer "look up first, call
        // ChangeType" rule to confirm the DATABASE itself also enforces one
        // reaction per user per post (same belt-and-braces reasoning as
        // CommentReactionConfiguration's own test).
        await repository.AddAsync(PostReaction.Create(postId, userId, ReactionType.Angry));
        var act = async () => await context.SaveChangesAsync();
 
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}