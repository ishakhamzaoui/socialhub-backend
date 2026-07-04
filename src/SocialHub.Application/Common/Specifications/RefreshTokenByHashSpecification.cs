using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Common.Specifications;
 
public sealed class RefreshTokenByHashSpecification : BaseSpecification<RefreshToken>
{
    public RefreshTokenByHashSpecification(string tokenHash)
        : base(rt => rt.TokenHash == tokenHash)
    {
    }
}