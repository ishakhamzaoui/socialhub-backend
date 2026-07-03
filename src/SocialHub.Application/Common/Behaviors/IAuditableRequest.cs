namespace SocialHub.Application.Common.Behaviors;
 
/// <summary>
/// Implement on a command to have a successful execution recorded via
/// IAuditService. Innermost behavior — wraps the handler directly, so it
/// only ever records the outcome of a handler that actually ran.
/// </summary>
public interface IAuditableRequest
{
    string ActionName { get; }
}