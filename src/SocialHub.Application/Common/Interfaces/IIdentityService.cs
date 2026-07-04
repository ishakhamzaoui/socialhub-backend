using SocialHub.Application.Common.Models;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Abstracts ASP.NET Core Identity operations (UserManager/SignInManager)
/// behind an Application-layer interface, so command handlers never
/// reference SocialHub.Identity directly. Implemented by IdentityService in
/// SocialHub.Identity, registered after AddApplication() in Program.cs.
/// </summary>
public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default);
 
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default);
 
    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
 
    Task<Result<UserAuthInfo>> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
 
    Task<Result<UserAuthInfo>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Returns Success(null) when the email isn't registered — deliberately
    /// not a failure, so ForgotPasswordCommandHandler can't be used to probe
    /// which emails exist (it always returns a generic success to the caller
    /// either way).
    /// </summary>
    Task<Result<string?>> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
 
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
}