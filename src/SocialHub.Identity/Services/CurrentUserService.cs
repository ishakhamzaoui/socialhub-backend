using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Identity.Services;
 
/// <summary>
/// Real, HttpContext/claims-backed ICurrentUserService, overriding Phase 1's
/// NullCurrentUserService (last DI registration wins — see
/// AddIdentityAuthServices, called after AddApplication() in Program.cs).
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
 
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
 
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
 
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
 
    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}