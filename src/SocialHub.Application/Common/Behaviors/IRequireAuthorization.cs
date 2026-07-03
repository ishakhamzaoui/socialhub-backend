namespace SocialHub.Application.Common.Behaviors;
 
/// <summary>
/// Implement on a command/query to require an authenticated caller.
/// Optionally restrict to specific roles; leave Roles null to require only
/// authentication, not a specific role.
/// </summary>
public interface IRequireAuthorization
{
    string[]? Roles => null;
}