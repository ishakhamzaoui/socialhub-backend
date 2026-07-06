namespace SocialHub.Application.Common.Interfaces;
 
public sealed record ImageDimensions(int Width, int Height);
 
/// <summary>
/// Image resize/compression/thumbnail generation (roadmap 4.4, 4.6).
/// Implemented by SocialHub.Infrastructure.Media.SkiaImageProcessingService
/// using SkiaSharp (MIT-licensed — see Phase 4 context doc for why this was
/// chosen over SixLabors.ImageSharp, which is not unconditionally free for
/// commercial use per its Split License).
///
/// Deliberately typed with only primitive/BCL types (paths, ints) so that no
/// SkiaSharp type ever needs to appear in an Application-layer signature —
/// same boundary discipline as ITokenService/IIdentityService from Phase 3
/// (see onboarding doc §4.3).
/// </summary>
public interface IImageProcessingService
{
    Task<ImageDimensions> GetDimensionsAsync(string absoluteSourcePath, CancellationToken cancellationToken = default);
 
    /// <summary>Resizes (preserving aspect ratio, longest side capped at maxDimension) and re-encodes for compression. Returns the resulting dimensions.</summary>
    Task<ImageDimensions> ResizeAndCompressAsync(string absoluteSourcePath, string absoluteDestinationPath, int maxDimension, CancellationToken cancellationToken = default);
 
    Task GenerateThumbnailAsync(string absoluteSourcePath, string absoluteDestinationPath, int thumbnailSize, CancellationToken cancellationToken = default);
}