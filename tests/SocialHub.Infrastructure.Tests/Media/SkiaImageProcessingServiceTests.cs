using FluentAssertions;
using SkiaSharp;
using SocialHub.Infrastructure.Media;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Media;
 
/// <summary>
/// Real SkiaSharp decode/resize/encode — no mocking, since the entire point
/// of this class is correct image processing, which a mock can't verify.
/// Generates its own tiny solid-color test PNGs rather than committing
/// binary fixtures.
/// </summary>
public sealed class SkiaImageProcessingServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"socialhub-image-test-{Guid.NewGuid():N}");
    private readonly SkiaImageProcessingService _sut = new();
 
    public SkiaImageProcessingServiceTests()
    {
        Directory.CreateDirectory(_tempDir);
    }
 
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
 
    private string CreateTestImage(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.png");
 
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.CornflowerBlue);
        }
 
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fileStream = File.OpenWrite(path);
        data.SaveTo(fileStream);
 
        return path;
    }
 
    [Fact]
    public async Task GetDimensionsAsync_Should_ReturnActualImageDimensions()
    {
        var sourcePath = CreateTestImage(400, 300);
 
        var dimensions = await _sut.GetDimensionsAsync(sourcePath);
 
        dimensions.Width.Should().Be(400);
        dimensions.Height.Should().Be(300);
    }
 
    [Fact]
    public async Task ResizeAndCompressAsync_Should_ScaleDownToFitMaxDimension_PreservingAspectRatio()
    {
        var sourcePath = CreateTestImage(4000, 2000);
        var destPath = Path.Combine(_tempDir, "resized.jpg");
 
        var dimensions = await _sut.ResizeAndCompressAsync(sourcePath, destPath, maxDimension: 2000);
 
        dimensions.Width.Should().Be(2000);
        dimensions.Height.Should().Be(1000);
        File.Exists(destPath).Should().BeTrue();
 
        var actual = await _sut.GetDimensionsAsync(destPath);
        actual.Should().Be(dimensions);
    }
 
    [Fact]
    public async Task ResizeAndCompressAsync_Should_LeaveImagesSmallerThanTheCapUnscaled()
    {
        var sourcePath = CreateTestImage(200, 100);
        var destPath = Path.Combine(_tempDir, "unscaled.jpg");
 
        var dimensions = await _sut.ResizeAndCompressAsync(sourcePath, destPath, maxDimension: 2048);
 
        dimensions.Width.Should().Be(200);
        dimensions.Height.Should().Be(100);
    }
 
    [Fact]
    public async Task GenerateThumbnailAsync_Should_ProduceImageCappedAtThumbnailSize()
    {
        var sourcePath = CreateTestImage(1200, 800);
        var thumbnailPath = Path.Combine(_tempDir, "thumb.jpg");
 
        await _sut.GenerateThumbnailAsync(sourcePath, thumbnailPath, thumbnailSize: 320);
 
        File.Exists(thumbnailPath).Should().BeTrue();
 
        var dimensions = await _sut.GetDimensionsAsync(thumbnailPath);
        dimensions.Width.Should().Be(320);
        dimensions.Height.Should().Be(213); // 800 * (320/1200), rounded
    }
 
    [Fact]
    public async Task GetDimensionsAsync_Should_Throw_When_FileIsNotAnImage()
    {
        var notAnImagePath = Path.Combine(_tempDir, "not-an-image.txt");
        await File.WriteAllTextAsync(notAnImagePath, "hello");
 
        var act = async () => await _sut.GetDimensionsAsync(notAnImagePath);
 
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}