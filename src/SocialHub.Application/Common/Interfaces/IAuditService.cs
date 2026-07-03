namespace SocialHub.Application.Common.Interfaces;
 
public sealed record AuditEntry(string Action, string? UserId, DateTime OccurredOn);
 
public interface IAuditService
{
    Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}