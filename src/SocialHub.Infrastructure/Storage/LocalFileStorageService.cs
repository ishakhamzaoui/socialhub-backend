using Microsoft.Extensions.Options;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Media;
 
namespace SocialHub.Infrastructure.Storage;
 
/// <summary>
/// Native filesystem implementation of IFileStorageService, per spec §22.
/// Registered as a singleton (see Infrastructure/DependencyInjection.cs) —
/// it holds no per-request state, only the configured root path.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;
 
    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
 
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "temp"));
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "thumbnails"));
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "users"));
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "posts"));
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "messages"));
        Directory.CreateDirectory(Path.Combine(_options.RootPath, "communities"));
    }
 
    public async Task<string> SaveToTempAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid():N}{fileExtension}";
        var relativePath = $"temp/{fileName}";
        var absolutePath = GetAbsolutePath(relativePath);
 
        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
        await content.CopyToAsync(fileStream, cancellationToken);
 
        return relativePath;
    }
 
    public Task<(string FinalPath, string? FinalThumbnailPath)> PromoteAsync(
        string tempRelativePath,
        string? tempThumbnailRelativePath,
        Guid ownerId,
        MediaCategory category,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
 
        var extension = Path.GetExtension(fileName);
        var categoryFolder = CategoryFolder(category);
        var ownerFolder = ownerId.ToString("N");
        var finalDir = Path.Combine(_options.RootPath, categoryFolder, ownerFolder);
        Directory.CreateDirectory(finalDir);
 
        var finalFileName = $"{Guid.NewGuid():N}{extension}";
        var finalRelativePath = $"{categoryFolder}/{ownerFolder}/{finalFileName}";
        var finalAbsolutePath = Path.Combine(finalDir, finalFileName);
 
        File.Move(GetAbsolutePath(tempRelativePath), finalAbsolutePath);
 
        string? finalThumbnailRelativePath = null;
        if (tempThumbnailRelativePath is not null)
        {
            var thumbnailExtension = Path.GetExtension(tempThumbnailRelativePath);
            var thumbnailFileName = $"{Guid.NewGuid():N}{thumbnailExtension}";
            finalThumbnailRelativePath = $"thumbnails/{thumbnailFileName}";
 
            File.Move(GetAbsolutePath(tempThumbnailRelativePath), GetAbsolutePath(finalThumbnailRelativePath));
        }
 
        return Task.FromResult((finalRelativePath, finalThumbnailRelativePath));
    }
 
    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
 
        return Task.CompletedTask;
    }
 
    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
        return Task.FromResult(stream);
    }
 
    public string GetAbsolutePath(string relativePath) =>
        Path.Combine(_options.RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
 
    public Task<IReadOnlyList<string>> ListTempFilesOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.Combine(_options.RootPath, "temp");
        if (!Directory.Exists(tempDir))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
 
        IReadOnlyList<string> stalePaths = Directory.EnumerateFiles(tempDir)
            .Where(path => File.GetLastWriteTimeUtc(path) < cutoffUtc)
            .Select(path => $"temp/{Path.GetFileName(path)}")
            .ToList();
 
        return Task.FromResult(stalePaths);
    }
 
    private static string CategoryFolder(MediaCategory category) => category switch
    {
        MediaCategory.User => "users",
        MediaCategory.Post => "posts",
        MediaCategory.Message => "messages",
        MediaCategory.Community => "communities",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unmapped MediaCategory.")
    };
}