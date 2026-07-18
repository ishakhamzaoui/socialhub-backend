using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>Roadmap 6.2. Deliberately scoped to Content/Visibility only — attachments/hashtags/mentions are set at creation and not re-editable here (flagged scope boundary, see this script's header).</summary>
public sealed record UpdatePostCommand(
    Guid PostId,
    string? Content,
    PostVisibility Visibility) : ICommand<PostDto>, IRequireAuthorization, ITransactionalRequest;