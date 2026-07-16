using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Users.Profile;
using SocialHub.Domain.Users;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 5.1-5.14: User Management. Grows across Phase 5's scripts —
/// this version (script 25) covers profile CRUD, privacy, theme, and
/// language only. Avatar/cover (script 26), follow/block/mute/suggested
/// users (script 27+) are added to this same controller in later scripts,
/// mirroring MediaController's single-controller-per-feature-area style.
///
/// [Authorize] at the class level plus IRequireAuthorization on every
/// command/query, per spec §2 (authorization enforced in the Application
/// layer via pipeline behaviors, not solely in controllers) — same defense
/// in depth as MediaController.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
 
    public UsersController(ISender sender)
    {
        _sender = sender;
    }
 
    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyProfileQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand(request.DisplayName, request.Bio, request.Location, request.Website);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("{userId:guid}/profile")]
    public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserProfileQuery(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPut("me/privacy")]
    public async Task<IActionResult> UpdatePrivacySettings([FromBody] UpdatePrivacySettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdatePrivacySettingsCommand(request.Visibility), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPut("me/theme")]
    public async Task<IActionResult> UpdateTheme([FromBody] UpdateThemeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateThemePreferenceCommand(request.Theme), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPut("me/language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateLanguagePreferenceCommand(request.Language), cancellationToken);
        return result.ToActionResult(this);
    }
}
 
public sealed class UpdateProfileRequest
{
    public string DisplayName { get; set; } = default!;
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
}
 
public sealed class UpdatePrivacySettingsRequest
{
    public ProfileVisibility Visibility { get; set; }
}
 
public sealed class UpdateThemeRequest
{
    public ThemePreference Theme { get; set; }
}
 
public sealed class UpdateLanguageRequest
{
    public string Language { get; set; } = default!;
}