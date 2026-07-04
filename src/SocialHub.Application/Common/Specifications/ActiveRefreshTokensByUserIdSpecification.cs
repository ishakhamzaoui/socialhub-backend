using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Specifications;
 
/// <summary>
/// Used both to list a user's active sessions (GetActiveSessionsQuery) and
/// to revoke an entire refresh-token family when reuse of an already-redeemed
/// token is detected (RefreshTokenCommandHandler).
/// </summary>
public sealed class ActiveRefreshTokensByUserIdSpecification : BaseSpecification<RefreshToken>
{
    public ActiveRefreshTokensByUserIdSpecification(Guid userId)
        : base(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
    {
        ApplyOrderByDescending(rt => rt.CreatedAtUtc);
    }
}