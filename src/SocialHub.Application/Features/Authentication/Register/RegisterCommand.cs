using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Authentication.Register;
 
public sealed record RegisterCommand(string Email, string Password) : ICommand<RegisterResponseDto>;
 
public sealed record RegisterResponseDto(Guid UserId, string Email);