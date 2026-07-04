namespace SocialHub.Application.Common.Models;
 
public sealed record UserAuthInfo(Guid Id, string Email, IReadOnlyList<string> Roles);