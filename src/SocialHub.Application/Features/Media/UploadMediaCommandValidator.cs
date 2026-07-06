using FluentValidation;
 
namespace SocialHub.Application.Features.Media;
 
/// <summary>
/// Roadmap 4.1 upload validation. Only image/video MIME types are accepted
/// in Phase 4 — MediaKind.Other exists in the Domain for future extensibility
/// (e.g. arbitrary message attachments in Phase 10) but nothing consumes it
/// yet, so this validator deliberately doesn't allow it through.
///
/// Size ceilings are hardcoded here rather than sourced from appsettings:
/// Application cannot depend on SocialHub.Infrastructure to read
/// StorageOptions (see this script's header), and these thresholds are
/// closer to a product/validation decision than an ops-tunable knob like
/// TempFileTtlHours. Adjust the constants directly if the limits need to
/// change.
/// </summary>
public sealed class UploadMediaCommandValidator : AbstractValidator<UploadMediaCommand>
{
    private const long MaxImageSizeBytes = 15L * 1024 * 1024;  // 15 MB
    private const long MaxVideoSizeBytes = 500L * 1024 * 1024; // 500 MB
 
    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
 
    private static readonly HashSet<string> AllowedVideoMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/quicktime", "video/webm"
    };
 
    public UploadMediaCommandValidator()
    {
        RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.MimeType).NotEmpty();
        RuleFor(x => x.SizeBytes).GreaterThan(0);
 
        RuleFor(x => x)
            .Must(BeAnAllowedMimeTypeWithinItsSizeLimit)
            .WithMessage("Only JPEG/PNG/WEBP/GIF images (up to 15 MB) or MP4/MOV/WEBM videos (up to 500 MB) are accepted.");
    }
 
    private static bool BeAnAllowedMimeTypeWithinItsSizeLimit(UploadMediaCommand command)
    {
        if (AllowedImageMimeTypes.Contains(command.MimeType))
        {
            return command.SizeBytes <= MaxImageSizeBytes;
        }
 
        if (AllowedVideoMimeTypes.Contains(command.MimeType))
        {
            return command.SizeBytes <= MaxVideoSizeBytes;
        }
 
        return false;
    }
}