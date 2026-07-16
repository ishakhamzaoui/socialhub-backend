using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Users.Profile;
 
/// <summary>
/// Roadmap 5.9. Admin-only — Roles restricts at the Application-layer
/// pipeline (AuthorizationBehavior), in addition to the controller's
/// [Authorize(Policy = Permissions.Users.Manage)] attribute — same defense-
/// in-depth pattern as every other command/query in this project (spec §2).
/// Not user-settable; there is deliberately no self-service verification
/// request flow in Phase 5.
/// </summary>
public sealed record VerifyUserCommand(Guid TargetUserId, bool IsVerified) : ICommand, IRequireAuthorization, ITransactionalRequest
{
    public string[]? Roles => new[] { "Admin" };
}