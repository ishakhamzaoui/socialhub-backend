using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Application.Common.Defaults;
 
/// <summary>
/// Default IAuditService until a persistent audit trail is implemented
/// (Phase 21/Security work). No-ops so the Audit pipeline behavior is safe
/// to run from day one.
/// </summary>
public sealed class NullAuditService : IAuditService
{
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
}