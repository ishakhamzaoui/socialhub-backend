using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Application.Common.Defaults;
 
/// <summary>
/// Default ICurrentUserService until Phase 3 (Identity) registers the real,
/// HttpContext-backed implementation. Always reports an anonymous caller.
/// </summary>
public sealed class NullCurrentUserService : ICurrentUserService
{
    public string? UserId => null;
 
    public bool IsAuthenticated => false;
 
    public IReadOnlyCollection<string> Roles => Array.Empty<string>();
}