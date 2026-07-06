using FluentAssertions;
using Microsoft.Extensions.Options;
using SocialHub.Domain.Media;
using SocialHub.Infrastructure.Storage;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Storage;
 
/// <summary>
/// Exercises the real filesystem against a unique temp directory per test
/// instance (xUnit creates a fresh class instance per [Fact], so the
/// constructor/Dispose pair gives clean isolation without a shared fixture).
/// Never touches /var/socialhub — RootPath is always overridden here.
/// </summary>
public sealed class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _rootPath;
    private readonly LocalFileStorageService _sut;
 
    public LocalFileStorageServiceTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"socialhub-storage-test-{Guid.NewGuid():N}");
        _sut = new LocalFileStorageService(Options.Create(new StorageOptions { RootPath = _rootPath, TempFileTtlHours = 24 }));
    }
 
    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }
 
    [Fact]
    public async Task SaveToTempAsync_Should_WriteFileUnderTempFolder()
    {
        await using var content = new MemoryStream([1, 2, 3]);
 
        var relativePath = await _sut.SaveToTempAsync(content, ".jpg");
 
        relativePath.Should().StartWith("temp/");
        File.Exists(_sut.GetAbsolutePath(relativePath)).Should().BeTrue();
    }
 
    [Fact]
    public async Task PromoteAsync_Should_MoveFileToCategoryFolder_And_RemoveTempCopy()
    {
        await using var content = new MemoryStream([1, 2, 3]);
        var tempPath = await _sut.SaveToTempAsync(content, ".jpg");
        var ownerId = Guid.NewGuid();
 
        var (finalPath, finalThumbnailPath) = await _sut.PromoteAsync(
            tempPath, null, ownerId, MediaCategory.User, "photo.jpg", CancellationToken.None);
 
        finalPath.Should().StartWith($"users/{ownerId:N}/");
        finalThumbnailPath.Should().BeNull();
        File.Exists(_sut.GetAbsolutePath(finalPath)).Should().BeTrue();
        File.Exists(_sut.GetAbsolutePath(tempPath)).Should().BeFalse();
    }
 
    [Fact]
    public async Task PromoteAsync_Should_AlsoMoveThumbnail_When_Provided()
    {
        await using var content = new MemoryStream([1]);
        var tempPath = await _sut.SaveToTempAsync(content, ".jpg");
        await using var thumbnailContent = new MemoryStream([2]);
        var tempThumbnailPath = await _sut.SaveToTempAsync(thumbnailContent, ".jpg");
 
        var (_, finalThumbnailPath) = await _sut.PromoteAsync(
            tempPath, tempThumbnailPath, Guid.NewGuid(), MediaCategory.Post, "video.mp4", CancellationToken.None);
 
        finalThumbnailPath.Should().NotBeNull();
        finalThumbnailPath.Should().StartWith("thumbnails/");
        File.Exists(_sut.GetAbsolutePath(finalThumbnailPath!)).Should().BeTrue();
    }
 
    [Fact]
    public async Task DeleteAsync_Should_RemoveFile()
    {
        await using var content = new MemoryStream([1]);
        var relativePath = await _sut.SaveToTempAsync(content, ".jpg");
 
        await _sut.DeleteAsync(relativePath);
 
        File.Exists(_sut.GetAbsolutePath(relativePath)).Should().BeFalse();
    }
 
    [Fact]
    public async Task DeleteAsync_Should_NotThrow_When_FileDoesNotExist()
    {
        var act = async () => await _sut.DeleteAsync("temp/does-not-exist.jpg");
 
        await act.Should().NotThrowAsync();
    }
 
    [Fact]
    public async Task ListTempFilesOlderThanAsync_Should_ReturnOnlyFilesPastTheCutoff()
    {
        await using var oldContent = new MemoryStream([1]);
        var oldPath = await _sut.SaveToTempAsync(oldContent, ".jpg");
        File.SetLastWriteTimeUtc(_sut.GetAbsolutePath(oldPath), DateTime.UtcNow.AddDays(-2));
 
        await using var freshContent = new MemoryStream([2]);
        var freshPath = await _sut.SaveToTempAsync(freshContent, ".jpg");
 
        var stale = await _sut.ListTempFilesOlderThanAsync(DateTime.UtcNow.AddHours(-1));
 
        stale.Should().Contain(oldPath);
        stale.Should().NotContain(freshPath);
    }
}