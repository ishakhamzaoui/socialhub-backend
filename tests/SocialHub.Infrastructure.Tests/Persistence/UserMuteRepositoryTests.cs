using FluentAssertions;
using SocialHub.Domain.Users;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class UserMuteRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public UserMuteRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetMutedUserIdsAsync_Should_ReturnEveryUserMuted()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserMuteRepository(context);
 
        var muter = Guid.NewGuid();
        var muted1 = Guid.NewGuid();
        var muted2 = Guid.NewGuid();
        await repository.AddAsync(UserMute.Create(muter, muted1));
        await repository.AddAsync(UserMute.Create(muter, muted2));
        await context.SaveChangesAsync();
 
        var ids = await repository.GetMutedUserIdsAsync(muter);
 
        ids.Should().BeEquivalentTo(new[] { muted1, muted2 });
    }
 
    [Fact]
    public async Task IsMutedAsync_Should_ReturnFalse_AfterRemove()
    {
        await using var context = _fixture.CreateContext();
        var repository = new UserMuteRepository(context);
 
        var muter = Guid.NewGuid();
        var muted = Guid.NewGuid();
        var mute = UserMute.Create(muter, muted);
        await repository.AddAsync(mute);
        await context.SaveChangesAsync();
 
        repository.Remove(mute);
        await context.SaveChangesAsync();
 
        (await repository.IsMutedAsync(muter, muted)).Should().BeFalse();
    }
}