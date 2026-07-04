using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.ConfirmEmail;
 
public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand;