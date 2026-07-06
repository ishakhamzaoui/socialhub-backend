using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Media;
 
namespace SocialHub.Application.Features.Media;
 
/// <summary>Roadmap 4.1: Upload API. Content is the raw upload stream — the API layer owns opening/disposing it.</summary>
public sealed record UploadMediaCommand(
    Stream Content,
    string OriginalFileName,
    string MimeType,
    long SizeBytes,
    MediaCategory Category) : ICommand<MediaDto>, IRequireAuthorization, ITransactionalRequest;