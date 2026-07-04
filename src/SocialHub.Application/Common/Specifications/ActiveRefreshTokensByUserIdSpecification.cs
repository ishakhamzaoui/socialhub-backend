using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Specifications;
 
/// <summary>Used to revoke an entire refresh-token family when reuse of an already-redeemed token is detected.</summary>
public sealed class ActiveRefreshTokensByUserIdSpecification : BaseSpecification<RefreshToken>
{
    public ActiveRefreshTokensByUserIdSpecification(Guid userId)
        : base(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
    {
    }
}