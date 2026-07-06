namespace SocialHub.Domain.Media;
 
/// <summary>
/// Broad classification of an uploaded file, used to select which processing
/// pipeline runs (image resize/thumbnail vs. video metadata/thumbnail) and to
/// validate allowed MIME types at upload time.
/// </summary>
public enum MediaKind
{
    Image,
    Video,
    Other
}