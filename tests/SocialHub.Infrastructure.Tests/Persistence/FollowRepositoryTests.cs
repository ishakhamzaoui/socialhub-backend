using FluentAssertions;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class FollowRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public FollowRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_AfterFollowIsAdded()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FollowRepository(context);
 
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();
        await repository.AddAsync(Follow.Create(followerId, followingId));
        await context.SaveChangesAsync();
 
        var exists = await repository.ExistsAsync(followerId, followingId);
 
        exists.Should().BeTrue();
    }
 
    [Fact]
    public async Task GetFollowerIdsAsync_Should_ExcludeSpecifiedUserIds()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FollowRepository(context);
 
        var targetId = Guid.NewGuid();
        var follower1 = Guid.NewGuid();
        var follower2 = Guid.NewGuid();
        await repository.AddAsync(Follow.Create(follower1, targetId));
        await repository.AddAsync(Follow.Create(follower2, targetId));
        await context.SaveChangesAsync();
 
        var (ids, total) = await repository.GetFollowerIdsAsync(targetId, page: 1, pageSize: 20, excludeUserIds: new[] { follower1 });
 
        ids.Should().ContainSingle().Which.Should().Be(follower2);
        total.Should().Be(1); // exclusion is applied inside the count, not post-hoc
    }
 
    [Fact]
    public async Task GetSuggestedUserIdsAsync_Should_ReturnMutualFollowSuggestions()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FollowRepository(context);
 
        var me = Guid.NewGuid();
        var friend = Guid.NewGuid();
        var suggestion = Guid.NewGuid();
        await repository.AddAsync(Follow.Create(me, friend));
        await repository.AddAsync(Follow.Create(friend, suggestion));
        await context.SaveChangesAsync();
 
        var suggestions = await repository.GetSuggestedUserIdsAsync(me, limit: 10);
 
        suggestions.Should().Contain(suggestion);
        suggestions.Should().NotContain(friend); // already followed, must not be suggested
    }
 
    [Fact]
    public async Task RemoveBetweenAsync_Should_DeleteFollowRowsInEitherDirection()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FollowRepository(context);
 
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await repository.AddAsync(Follow.Create(userA, userB));
        await context.SaveChangesAsync();
 
        await repository.RemoveBetweenAsync(userA, userB);
        await context.SaveChangesAsync();
 
        var exists = await repository.ExistsAsync(userA, userB);
        exists.Should().BeFalse();
    }
}