using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialHub.Identity.Permissions;
 
namespace SocialHub.API.Extensions;
 
public static class AuthExtensions
{
    public static IServiceCollection AddSocialHubAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
 
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = string.IsNullOrWhiteSpace(jwtKey)
                    ? null
                    : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
 
        var authorizationBuilder = services.AddAuthorizationBuilder();
 
        // One policy per catalog permission (SocialHub.Identity.Permissions),
        // each requiring the matching "permission" claim minted into the JWT
        // at login/refresh (TokenService + IdentityService, roles seeded with
        // permission claims in ApplicationDbContextSeeder). Later phases that
        // append to Permissions.All need no change here — this loop covers
        // whatever the catalog currently contains.
        foreach (var permission in Permissions.All)
        {
            authorizationBuilder.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
        }
 
        return services;
    }
}