using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Identity.Models;
using SocialHub.Identity.Options;
using SocialHub.Identity.Services;
using SocialHub.Identity.Token;
 
namespace SocialHub.Identity;
 
public static class DependencyInjection
{
    /// <summary>
    /// Registers ASP.NET Core Identity core services — roles, the sign-in
    /// manager, and default token providers (used for email-confirmation /
    /// password-reset tokens).
    ///
    /// Uses AddIdentityCore rather than AddIdentity: AddIdentity also wires a
    /// cookie authentication scheme, which this API does not want — it's
    /// JWT-bearer-only (spec §21: no CSRF concerns because tokens never live
    /// in cookies), and AuthExtensions.AddSocialHubAuthentication() (Phase 0)
    /// already registers the JWT bearer scheme as the default. AddIdentityCore
    /// does not touch authentication schemes at all, so the two compose
    /// cleanly regardless of call order.
    ///
    /// Deliberately does NOT call .AddEntityFrameworkStores&lt;T&gt;() here:
    /// SocialHub.Identity must not reference SocialHub.Persistence (that
    /// would be circular, since Persistence already references Identity for
    /// ApplicationUser/ApplicationRole). The composition root (API's
    /// Program.cs) chains .AddEntityFrameworkStores&lt;ApplicationDbContext&gt;()
    /// onto the IdentityBuilder returned here.
    /// </summary>
    public static IdentityBuilder AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
 
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
 
                options.User.RequireUniqueEmail = true;
 
                // Login is blocked until email is confirmed (spec 15.1 /
                // roadmap 3.8). The dev-seeded admin has its email
                // pre-confirmed so local development isn't blocked on SMTP.
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
    }
 
    /// <summary>
    /// Registers JWT issuance (ITokenService), the real ICurrentUserService,
    /// IIdentityService (UserManager/SignInManager facade), and
    /// IAppUrlProvider. Must be called after AddApplication() in Program.cs
    /// so ICurrentUserService overrides Phase 1's NullCurrentUserService
    /// (last registration wins for constructor injection in Microsoft.DI).
    /// </summary>
    public static IServiceCollection AddIdentityAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<AppUrlOptions>(configuration.GetSection("AppUrls"));
 
        services.AddHttpContextAccessor();
 
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAppUrlProvider, AppUrlProvider>();
 
        return services;
    }
}