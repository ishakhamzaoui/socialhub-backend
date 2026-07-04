using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SocialHub.API.Contracts.Auth;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Authentication.ConfirmEmail;
using SocialHub.Application.Features.Authentication.ForgotPassword;
using SocialHub.Application.Features.Authentication.Login;
using SocialHub.Application.Features.Authentication.Logout;
using SocialHub.Application.Features.Authentication.RefreshToken;
using SocialHub.Application.Features.Authentication.Register;
using SocialHub.Application.Features.Authentication.ResetPassword;
using SocialHub.Application.Features.Authentication.Sessions;
 
namespace SocialHub.API.Controllers;
 
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
 
    public AuthController(ISender sender)
    {
        _sender = sender;
    }
 
    [EnableRateLimiting(RateLimitingExtensions.RegisterPolicy)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var command = new LoginCommand(
            request.Email,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            string.IsNullOrWhiteSpace(deviceName) ? null : deviceName);
 
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [EnableRateLimiting(RateLimitingExtensions.PasswordResetPolicy)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [EnableRateLimiting(RateLimitingExtensions.PasswordResetPolicy)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActiveSessionsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [Authorize]
    [HttpPost("sessions/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RevokeSessionCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}