using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.ForgotPassword;
 
public sealed record ForgotPasswordCommand(string Email) : ICommand;