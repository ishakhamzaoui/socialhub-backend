using Microsoft.AspNetCore.Identity;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Models;
using SocialHub.Application.Common.Results;
using SocialHub.Identity.Models;
using SocialHub.Identity.Permissions;
 
namespace SocialHub.Identity.Services;
 
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
 
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
 
    public async Task<Result<Guid>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return Result.Failure<Guid>(Error.Conflict("Auth.EmailAlreadyRegistered", "An account with this email already exists."));
        }
 
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
 
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(Error.Validation("Auth.RegistrationFailed", message));
        }
 
        await _userManager.AddToRoleAsync(user, "User");
 
        return Result.Success(user.Id);
    }
 
    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure<string>(Error.NotFound("Auth.UserNotFound", "User not found."));
        }
 
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return Result.Success(token);
    }
 
    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Auth.UserNotFound", "User not found."));
        }
 
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return Result.Failure(Error.Validation("Auth.EmailConfirmationFailed", "The confirmation link is invalid or has expired."));
        }
 
        return Result.Success();
    }
 
    public async Task<Result<UserAuthInfo>> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<UserAuthInfo>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }
 
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
 
        if (signInResult.IsLockedOut)
        {
            return Result.Failure<UserAuthInfo>(Error.Forbidden("Auth.LockedOut", "This account is temporarily locked due to repeated failed login attempts."));
        }
 
        if (signInResult.IsNotAllowed)
        {
            return Result.Failure<UserAuthInfo>(Error.Forbidden("Auth.EmailNotConfirmed", "Please confirm your email address before logging in."));
        }
 
        if (!signInResult.Succeeded)
        {
            return Result.Failure<UserAuthInfo>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }
 
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
 
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(roles);
 
        return Result.Success(new UserAuthInfo(user.Id, user.Email!, roles.ToList(), permissions));
    }
 
    public async Task<Result<UserAuthInfo>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result.Failure<UserAuthInfo>(Error.NotFound("Auth.UserNotFound", "User not found."));
        }
 
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(roles);
 
        return Result.Success(new UserAuthInfo(user.Id, user.Email!, roles.ToList(), permissions));
    }
 
    public async Task<Result<string?>> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Success<string?>(null);
        }
 
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Result.Success<string?>(token);
    }
 
    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Auth.UserNotFound", "User not found."));
        }
 
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Auth.PasswordResetFailed", message));
        }
 
        return Result.Success();
    }
 
    private async Task<IReadOnlyList<string>> GetPermissionsAsync(IEnumerable<string> roleNames)
    {
        var permissions = new HashSet<string>();
 
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }
 
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == Permissions.Permissions.ClaimType))
            {
                permissions.Add(claim.Value);
            }
        }
 
        return permissions.ToList();
    }
}