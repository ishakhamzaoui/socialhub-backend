using FluentAssertions;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class HashtagRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public HashtagRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_Should_ReturnPersistedHashtag()
    {
        await using var context = _fixture.CreateContext();
        var repository = new HashtagRepository(context);
 
        var hashtag = Hashtag.Create("CleanArchitecture");
        await repository.AddAsync(hashtag);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdAsync(hashtag.Id);
 
        loaded.Should().NotBeNull();
        loaded!.Tag.Should().Be("CleanArchitecture");
    }
 
    [Fact]
    public async Task GetByNormalizedTagAsync_Should_MatchRegardlessOfCase()
    {
        await using var context = _fixture.CreateContext();
        var repository = new HashtagRepository(context);
 
        var hashtag = Hashtag.Create("DotNet");
        await repository.AddAsync(hashtag);
        await context.SaveChangesAsync();
 
        var found = await repository.GetByNormalizedTagAsync("DOTNET");
 
        found.Should().NotBeNull();
        found!.Id.Should().Be(hashtag.Id);
    }
 
    [Fact]
    public async Task Update_Should_PersistChanges()
    {
        await using var context = _fixture.CreateContext();
        var repository = new HashtagRepository(context);
 
        var hashtag = Hashtag.Create("Original");
        await repository.AddAsync(hashtag);
        await context.SaveChangesAsync();
 
        hashtag.IncrementUsage();
        repository.Update(hashtag);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdAsync(hashtag.Id);
 
        loaded!.UsageCount.Should().Be(1);
    }
 
    [Fact]
    public async Task Remove_Should_DeleteHashtag()
    {
        await using var context = _fixture.CreateContext();
        var repository = new HashtagRepository(context);
 
        var hashtag = Hashtag.Create("ToDelete");
        await repository.AddAsync(hashtag);
        await context.SaveChangesAsync();
 
        repository.Remove(hashtag);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdAsync(hashtag.Id);
 
        loaded.Should().BeNull();
    }
 
    [Fact]
    public async Task ListAsync_WithSpecification_Should_ReturnOrderedResults()
    {
        await using var context = _fixture.CreateContext();
        var repository = new HashtagRepository(context);
 
        await repository.AddAsync(Hashtag.Create("Zebra"));
        await repository.AddAsync(Hashtag.Create("Alpha"));
        await context.SaveChangesAsync();
 
        var results = await repository.ListAsync(new SocialHub.Application.Common.Specifications.AllHashtagsSpecification());
 
        results.Select(h => h.Tag).Should().ContainInOrder("Alpha", "Zebra");
    }
}