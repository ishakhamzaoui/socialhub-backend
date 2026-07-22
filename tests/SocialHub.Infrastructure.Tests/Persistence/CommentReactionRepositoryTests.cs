using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class CommentReactionRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public CommentReactionRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetAsync_Should_ReturnTheUsersReactionOnThatComment()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentReactionRepository(context);
 
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reaction = CommentReaction.Create(commentId, userId, ReactionType.Laugh);
        await repository.AddAsync(reaction);
        await context.SaveChangesAsync();
 
        var result = await repository.GetAsync(commentId, userId);
 
        result.Should().NotBeNull();
        result!.Type.Should().Be(ReactionType.Laugh);
    }
 
    [Fact]
    public async Task GetCountsByTypeAsync_Should_AggregateCorrectlyAcrossUsers()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentReactionRepository(context);
 
        var commentId = Guid.NewGuid();
        await repository.AddAsync(CommentReaction.Create(commentId, Guid.NewGuid(), ReactionType.Like));
        await repository.AddAsync(CommentReaction.Create(commentId, Guid.NewGuid(), ReactionType.Like));
        await repository.AddAsync(CommentReaction.Create(commentId, Guid.NewGuid(), ReactionType.Sad));
        await context.SaveChangesAsync();
 
        var counts = await repository.GetCountsByTypeAsync(commentId);
 
        counts[ReactionType.Like].Should().Be(2);
        counts[ReactionType.Sad].Should().Be(1);
        counts.ContainsKey(ReactionType.Angry).Should().BeFalse();
    }
 
    [Fact]
    public async Task UniqueIndex_Should_RejectASecondReaction_ForSameCommentAndUser()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentReactionRepository(context);
 
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await repository.AddAsync(CommentReaction.Create(commentId, userId, ReactionType.Like));
        await context.SaveChangesAsync();
 
        // Deliberately bypassing the application-layer "look up first, call
        // ChangeType" rule to confirm the DATABASE itself also enforces one
        // reaction per user per comment (belt-and-braces — see
        // CommentReactionConfiguration's remarks).
        await repository.AddAsync(CommentReaction.Create(commentId, userId, ReactionType.Angry));
        var act = async () => await context.SaveChangesAsync();
 
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}