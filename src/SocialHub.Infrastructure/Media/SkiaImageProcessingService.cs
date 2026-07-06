using SkiaSharp;
using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Infrastructure.Media;
 
/// <summary>
/// Image dimension/resize/thumbnail processing (roadmap 4.4, 4.6) via
/// SkiaSharp (MIT). SixLabors.ImageSharp was deliberately not used here —
/// its Split License is not unconditionally free for commercial use above a
/// revenue threshold, which conflicts with spec §1's licensing constraint.
/// </summary>
public sealed class SkiaImageProcessingService : IImageProcessingService
{
    public Task<ImageDimensions> GetDimensionsAsync(string absoluteSourcePath, CancellationToken cancellationToken = default)
    {
        using var codec = SKCodec.Create(absoluteSourcePath)
            ?? throw new InvalidOperationException($"Unable to read image at '{absoluteSourcePath}'.");
 
        return Task.FromResult(new ImageDimensions(codec.Info.Width, codec.Info.Height));
    }
 
    public Task<ImageDimensions> ResizeAndCompressAsync(string absoluteSourcePath, string absoluteDestinationPath, int maxDimension, CancellationToken cancellationToken = default)
    {
        using var original = SKBitmap.Decode(absoluteSourcePath)
            ?? throw new InvalidOperationException($"Unable to decode image at '{absoluteSourcePath}'.");
 
        var (width, height) = ScaleToFit(original.Width, original.Height, maxDimension);
 
        EncodeResized(original, width, height, absoluteDestinationPath, quality: 82);
 
        return Task.FromResult(new ImageDimensions(width, height));
    }
 
    public Task GenerateThumbnailAsync(string absoluteSourcePath, string absoluteDestinationPath, int thumbnailSize, CancellationToken cancellationToken = default)
    {
        using var original = SKBitmap.Decode(absoluteSourcePath)
            ?? throw new InvalidOperationException($"Unable to decode image at '{absoluteSourcePath}'.");
 
        var (width, height) = ScaleToFit(original.Width, original.Height, thumbnailSize);
 
        EncodeResized(original, width, height, absoluteDestinationPath, quality: 75);
 
        return Task.CompletedTask;
    }
 
    private static void EncodeResized(SKBitmap original, int width, int height, string absoluteDestinationPath, int quality)
    {
        using var resized = original.Resize(new SKImageInfo(width, height), SKSamplingOptions.Default)
            ?? throw new InvalidOperationException("Image resize failed.");
 
        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
 
        var directory = Path.GetDirectoryName(absoluteDestinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
 
        using var fileStream = File.OpenWrite(absoluteDestinationPath);
        data.SaveTo(fileStream);
    }
 
    private static (int Width, int Height) ScaleToFit(int width, int height, int maxDimension)
    {
        if (width <= maxDimension && height <= maxDimension)
        {
            return (width, height);
        }
 
        var scale = width >= height
            ? (double)maxDimension / width
            : (double)maxDimension / height;
 
        return (Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
    }
}