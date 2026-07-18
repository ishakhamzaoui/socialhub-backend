using FluentAssertions;
using SocialHub.Domain.Posts;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class PostRepostRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public PostRepostRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_AfterRepostIsAdded()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepostRepository(context);
 
        var userId = Guid.NewGuid();
        var originalPostId = Guid.NewGuid();
        await repository.AddAsync(PostRepost.Create(userId, originalPostId));
        await context.SaveChangesAsync();
 
        var exists = await repository.ExistsAsync(userId, originalPostId);
 
        exists.Should().BeTrue();
    }
 
    [Fact]
    public async Task GetRepostCountAsync_Should_CountAllRepostsOfAPost()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepostRepository(context);
 
        var originalPostId = Guid.NewGuid();
        await repository.AddAsync(PostRepost.Create(Guid.NewGuid(), originalPostId));
        await repository.AddAsync(PostRepost.Create(Guid.NewGuid(), originalPostId));
        await context.SaveChangesAsync();
 
        var count = await repository.GetRepostCountAsync(originalPostId);
 
        count.Should().Be(2);
    }
 
    [Fact]
    public async Task RemoveAsync_Should_DeleteTheRow()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepostRepository(context);
 
        var userId = Guid.NewGuid();
        var originalPostId = Guid.NewGuid();
        await repository.AddAsync(PostRepost.Create(userId, originalPostId));
        await context.SaveChangesAsync();
 
        await repository.RemoveAsync(userId, originalPostId);
        await context.SaveChangesAsync();
 
        var exists = await repository.ExistsAsync(userId, originalPostId);
        exists.Should().BeFalse();
    }
 
    [Fact]
    public async Task RemoveAsync_Should_BeNoOp_When_RowDoesNotExist()
    {
        await using var context = _fixture.CreateContext();
        var repository = new PostRepostRepository(context);
 
        var act = async () =>
        {
            await repository.RemoveAsync(Guid.NewGuid(), Guid.NewGuid());
            await context.SaveChangesAsync();
        };
 
        await act.Should().NotThrowAsync();
    }
}