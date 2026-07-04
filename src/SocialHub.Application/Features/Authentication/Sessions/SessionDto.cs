namespace SocialHub.Application.Features.Authentication.Sessions;
 
public sealed record SessionDto(Guid Id, string? DeviceName, string? CreatedByIp, DateTime CreatedAtUtc, DateTime ExpiresAtUtc);