using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Application.Features.Users.Follow;
using SocialHub.Application.Features.Users.Profile;
using SocialHub.Application.Features.Users.Safety;
using SocialHub.Domain.Media;
using SocialHub.Domain.Users;
using SocialHub.Identity.Permissions;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 5.1-5.14: User Management — complete as of script 28. Grew
/// across scripts 25 (profile/privacy/theme/language), 26 (avatar/cover),
/// 27 (follow graph), 28 (block/mute/verification).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorageService;
 
    public UsersController(ISender sender, IFileStorageService fileStorageService)
    {
        _sender = sender;
        _fileStorageService = fileStorageService;
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
 
    [HttpPost("me/avatar")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "A file is required.");
        }
 
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "An avatar must be an image file.");
        }
 
        await using var stream = file.OpenReadStream();
        var uploadResult = await _sender.Send(
            new UploadMediaCommand(stream, file.FileName, file.ContentType, file.Length, MediaCategory.User),
            cancellationToken);
 
        if (uploadResult.IsFailure)
        {
            return uploadResult.ToActionResult(this);
        }
 
        var setResult = await _sender.Send(new SetAvatarCommand(uploadResult.Value.Id), cancellationToken);
        if (setResult.IsFailure)
        {
            return setResult.ToActionResult(this);
        }
 
        var profileResult = await _sender.Send(new GetMyProfileQuery(), cancellationToken);
        return profileResult.ToActionResult(this);
    }
 
    [HttpPost("me/cover")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "A file is required.");
        }
 
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "A cover image must be an image file.");
        }
 
        await using var stream = file.OpenReadStream();
        var uploadResult = await _sender.Send(
            new UploadMediaCommand(stream, file.FileName, file.ContentType, file.Length, MediaCategory.User),
            cancellationToken);
 
        if (uploadResult.IsFailure)
        {
            return uploadResult.ToActionResult(this);
        }
 
        var setResult = await _sender.Send(new SetCoverCommand(uploadResult.Value.Id), cancellationToken);
        if (setResult.IsFailure)
        {
            return setResult.ToActionResult(this);
        }
 
        var profileResult = await _sender.Send(new GetMyProfileQuery(), cancellationToken);
        return profileResult.ToActionResult(this);
    }
 
    [HttpGet("{userId:guid}/avatar")]
    public async Task<IActionResult> GetAvatar(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserAvatarFileQuery(userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }
 
        var stream = await _fileStorageService.OpenReadAsync(result.Value.RelativePath, cancellationToken);
        return File(stream, result.Value.MimeType, result.Value.FileName);
    }
 
    [HttpGet("{userId:guid}/cover")]
    public async Task<IActionResult> GetCover(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserCoverFileQuery(userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }
 
        var stream = await _fileStorageService.OpenReadAsync(result.Value.RelativePath, cancellationToken);
        return File(stream, result.Value.MimeType, result.Value.FileName);
    }
 
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FollowUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{userId:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnfollowUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetFollowersQuery(userId, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetFollowingQuery(userId, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("me/suggested")]
    public async Task<IActionResult> GetSuggestedUsers([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetSuggestedUsersQuery(limit), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPost("{userId:guid}/block")]
    public async Task<IActionResult> Block(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new BlockUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{userId:guid}/block")]
    public async Task<IActionResult> Unblock(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnblockUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("me/blocked")]
    public async Task<IActionResult> GetBlockedUsers(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBlockedUsersQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPost("{userId:guid}/mute")]
    public async Task<IActionResult> Mute(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MuteUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{userId:guid}/mute")]
    public async Task<IActionResult> Unmute(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnmuteUserCommand(userId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("me/muted")]
    public async Task<IActionResult> GetMutedUsers(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMutedUsersQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 5.9. Policy check here is defense in depth alongside VerifyUserCommand's own Roles restriction (see its remarks).</summary>
    [HttpPut("{userId:guid}/verification")]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> SetVerification(Guid userId, [FromBody] SetVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new VerifyUserCommand(userId, request.IsVerified), cancellationToken);
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
 
public sealed class SetVerificationRequest
{
    public bool IsVerified { get; set; }
}