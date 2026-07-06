using FluentAssertions;
using SocialHub.Domain.Media;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class MediaAssetRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public MediaAssetRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    private static MediaAsset CreateSampleAsset(Guid ownerId) =>
        MediaAsset.Create(
            ownerId,
            MediaKind.Image,
            MediaCategory.User,
            "avatar.jpg",
            $"users/{ownerId:N}/{Guid.NewGuid():N}.jpg",
            $"thumbnails/{Guid.NewGuid():N}.jpg",
            "image/jpeg",
            sizeBytes: 12345,
            widthPx: 800,
            heightPx: 600);
 
    [Fact]
    public async Task AddAsync_Then_GetByIdForOwnerAsync_Should_ReturnPersistedAsset()
    {
        await using var context = _fixture.CreateContext();
        var repository = new MediaAssetRepository(context);
        var ownerId = Guid.NewGuid();
 
        var asset = CreateSampleAsset(ownerId);
        await repository.AddAsync(asset);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdForOwnerAsync(asset.Id, ownerId);
 
        loaded.Should().NotBeNull();
        loaded!.OriginalFileName.Should().Be("avatar.jpg");
        loaded.Kind.Should().Be(MediaKind.Image);
        loaded.Category.Should().Be(MediaCategory.User);
    }
 
    [Fact]
    public async Task GetByIdForOwnerAsync_Should_ReturnNull_When_AssetBelongsToAnotherOwner()
    {
        await using var context = _fixture.CreateContext();
        var repository = new MediaAssetRepository(context);
        var asset = CreateSampleAsset(Guid.NewGuid());
        await repository.AddAsync(asset);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdForOwnerAsync(asset.Id, Guid.NewGuid());
 
        loaded.Should().BeNull();
    }
 
    [Fact]
    public async Task GetByOwnerAsync_Should_ReturnAllAssetsForThatOwner_MostRecentFirst()
    {
        await using var context = _fixture.CreateContext();
        var repository = new MediaAssetRepository(context);
        var ownerId = Guid.NewGuid();
 
        var first = CreateSampleAsset(ownerId);
        await repository.AddAsync(first);
        await context.SaveChangesAsync();
 
        await Task.Delay(10); // ensure a distinct CreatedAtUtc for a deterministic order
 
        var second = CreateSampleAsset(ownerId);
        await repository.AddAsync(second);
        await context.SaveChangesAsync();
 
        var results = await repository.GetByOwnerAsync(ownerId);
 
        results.Select(a => a.Id).Should().ContainInOrder(second.Id, first.Id);
    }
 
    [Fact]
    public async Task Remove_Should_DeleteTheRow()
    {
        await using var context = _fixture.CreateContext();
        var repository = new MediaAssetRepository(context);
        var asset = CreateSampleAsset(Guid.NewGuid());
        await repository.AddAsync(asset);
        await context.SaveChangesAsync();
 
        repository.Remove(asset);
        await context.SaveChangesAsync();
 
        var loaded = await repository.GetByIdAsync(asset.Id);
        loaded.Should().BeNull();
    }
}