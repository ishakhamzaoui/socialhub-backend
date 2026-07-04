using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.ResetPassword;
 
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;