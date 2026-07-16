using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>Same shape and rules as SetAvatarCommand, for the cover image.</summary>
public sealed record SetCoverCommand(Guid MediaAssetId) : ICommand, IRequireAuthorization, ITransactionalRequest;