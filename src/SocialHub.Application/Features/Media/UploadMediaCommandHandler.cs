using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Media;
 
namespace SocialHub.Application.Features.Media;
 
public sealed class UploadMediaCommandHandler : ICommandHandler<UploadMediaCommand, MediaDto>
{
    // See UploadMediaCommandValidator's remarks for why these live here as
    // constants rather than in Infrastructure's StorageOptions.
    private const int ImageMaxDimension = 2048;
    private const int ThumbnailSize = 320;
 
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UploadMediaCommandHandler(
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IImageProcessingService imageProcessingService,
        IVideoProcessingService videoProcessingService,
        IMediaAssetRepository mediaAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _videoProcessingService = videoProcessingService;
        _mediaAssetRepository = mediaAssetRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<MediaDto>> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        // AuthorizationBehavior (IRequireAuthorization) already guarantees an
        // authenticated caller, but ICurrentUserService.UserId is string? by
        // contract (onboarding doc §4.3), so the parse still has to happen here.
        if (!Guid.TryParse(_currentUserService.UserId, out var ownerId))
        {
            return Result.Failure<MediaDto>(
                Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required to upload media."));
        }
 
        var kind = ResolveKind(request.MimeType);
 
        var extension = Path.GetExtension(request.OriginalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = kind == MediaKind.Image ? ".jpg" : ".mp4";
        }
 
        var tempPath = await _fileStorageService.SaveToTempAsync(request.Content, extension, cancellationToken);
        string? tempThumbnailPath = null;
 
        try
        {
            int? width = null;
            int? height = null;
            double? durationSeconds = null;
 
            var absoluteTempPath = _fileStorageService.GetAbsolutePath(tempPath);
 
            if (kind == MediaKind.Image)
            {
                // Resize/compress in place over the temp file (roadmap 4.4),
                // so the file that eventually gets promoted is already the
                // web-optimized version, not the raw upload.
                var dimensions = await _imageProcessingService.ResizeAndCompressAsync(
                    absoluteTempPath, absoluteTempPath, ImageMaxDimension, cancellationToken);
                width = dimensions.Width;
                height = dimensions.Height;
 
                tempThumbnailPath = $"temp/{Guid.NewGuid():N}.jpg";
                await _imageProcessingService.GenerateThumbnailAsync(
                    absoluteTempPath, _fileStorageService.GetAbsolutePath(tempThumbnailPath), ThumbnailSize, cancellationToken);
            }
            else if (kind == MediaKind.Video)
            {
                var metadata = await _videoProcessingService.ExtractMetadataAsync(absoluteTempPath, cancellationToken);
                width = metadata.Width;
                height = metadata.Height;
                durationSeconds = metadata.DurationSeconds;
 
                tempThumbnailPath = $"temp/{Guid.NewGuid():N}.jpg";
                await _videoProcessingService.GenerateThumbnailAsync(
                    absoluteTempPath, _fileStorageService.GetAbsolutePath(tempThumbnailPath), cancellationToken);
            }
 
            var (finalPath, finalThumbnailPath) = await _fileStorageService.PromoteAsync(
                tempPath, tempThumbnailPath, ownerId, request.Category, request.OriginalFileName, cancellationToken);
 
            var asset = MediaAsset.Create(
                ownerId,
                kind,
                request.Category,
                request.OriginalFileName,
                finalPath,
                finalThumbnailPath,
                request.MimeType,
                request.SizeBytes,
                width,
                height,
                durationSeconds);
 
            await _mediaAssetRepository.AddAsync(asset, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
 
            return Result.Success(MediaDto.From(asset));
        }
        catch
        {
            // Processing or promotion failed partway. Best-effort cleanup of
            // whatever's still under temp/ (DeleteAsync is a no-op for paths
            // that no longer exist, e.g. if PromoteAsync already moved them).
            //
            // Known gap: if PromoteAsync moves the main file successfully but
            // then throws while moving the thumbnail, the main file is left
            // sitting in its final category folder with no MediaAsset row —
            // MediaCleanupService only sweeps temp/, so it won't catch this.
            // In practice a same-filesystem File.Move failing after an
            // earlier one succeeded is extremely unlikely; flagging as a
            // known limitation for the Phase 4 context doc rather than adding
            // two-phase-commit machinery for it now.
            await _fileStorageService.DeleteAsync(tempPath, cancellationToken);
            if (tempThumbnailPath is not null)
            {
                await _fileStorageService.DeleteAsync(tempThumbnailPath, cancellationToken);
            }
 
            throw;
        }
    }
 
    private static MediaKind ResolveKind(string mimeType) =>
        mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ? MediaKind.Image
        : mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? MediaKind.Video
        : MediaKind.Other;
}