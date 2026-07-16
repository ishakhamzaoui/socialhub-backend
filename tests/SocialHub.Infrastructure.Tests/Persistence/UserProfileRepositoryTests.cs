using FluentAssertions;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class UserProfileRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public UserProfileRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnPersistedProfile()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserProfileRepository(context);
 
        var userId = Guid.NewGuid();
        var profile = UserProfile.CreateDefault(userId, "alice");
        await repository.AddAsync(profile);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByUserIdAsync(userId);
 
        loaded.Should().NotBeNull();
        loaded!.DisplayName.Should().Be("alice");
    }
 
    [Fact]
    public async Task GetByUserIdsAsync_Should_ReturnAllMatchingProfiles()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserProfileRepository(context);
 
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        await repository.AddAsync(UserProfile.CreateDefault(userId1, "alice"));
        await repository.AddAsync(UserProfile.CreateDefault(userId2, "bob"));
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByUserIdsAsync(new[] { userId1, userId2, Guid.NewGuid() });
 
        loaded.Should().HaveCount(2);
    }
}