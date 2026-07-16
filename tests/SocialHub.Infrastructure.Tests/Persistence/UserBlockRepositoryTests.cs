using FluentAssertions;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class UserBlockRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public UserBlockRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task IsBlockedEitherDirectionAsync_Should_ReturnTrue_RegardlessOfWhoBlockedWhom()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserBlockRepository(context);
 
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await repository.AddAsync(UserBlock.Create(userA, userB));
        await context.SaveChangesAsync();
 
        (await repository.IsBlockedEitherDirectionAsync(userA, userB)).Should().BeTrue();
        (await repository.IsBlockedEitherDirectionAsync(userB, userA)).Should().BeTrue();
    }
 
    [Fact]
    public async Task GetBlockedUserIdsAsync_And_GetBlockedByUserIdsAsync_Should_BeDirectional()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserBlockRepository(context);
 
        var blocker = Guid.NewGuid();
        var blocked = Guid.NewGuid();
        await repository.AddAsync(UserBlock.Create(blocker, blocked));
        await context.SaveChangesAsync();
 
        (await repository.GetBlockedUserIdsAsync(blocker)).Should().Contain(blocked);
        (await repository.GetBlockedByUserIdsAsync(blocked)).Should().Contain(blocker);
        (await repository.GetBlockedUserIdsAsync(blocked)).Should().BeEmpty();
    }
}