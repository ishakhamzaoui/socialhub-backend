using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialHub.Domain.Shared;
using SocialHub.Identity.Models;
using SocialHub.Persistence.Context;
 
namespace SocialHub.Persistence.Seed;
 
public static class ApplicationDbContextSeeder
{
    private static readonly string[] DefaultHashtags =
    {
        "welcome", "introduction", "general", "announcements", "help"
    };

    private static readonly (string Name, string? Description)[] DefaultRoles =
    {
        ("Admin", "Full administrative access"),
        ("Moderator", "Content moderation access"),
        ("User", "Standard authenticated user")
    };

    // DEV-ONLY credential, same pattern as the Postgres dev password from
    // Phase 0 — never used in production. Phase 3's real registration flow
    // (script 14) is how production admins actually get created.
    private const string DevAdminEmail = "admin@shub.lan";
    private const string DevAdminPassword = "Dev@Admin123!";

 
    public static async Task SeedAsync(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken = default)
    {
        await SeedHashtagsAsync(context, cancellationToken);
        await SeedRolesAsync(roleManager);
        await SeedDevAdminAsync(userManager);
    }
 
    private static async Task SeedHashtagsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Hashtags.AnyAsync(cancellationToken))
        {
            return;
        }
 
        var hashtags = DefaultHashtags.Select(Hashtag.Create).ToList();
        await context.Hashtags.AddRangeAsync(hashtags, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
 
    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var (name, description) in DefaultRoles)
        {
            if (await roleManager.RoleExistsAsync(name))
            {
                continue;
            }
 
            await roleManager.CreateAsync(new ApplicationRole(name, description));
        }
    }
 
    private static async Task SeedDevAdminAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(DevAdminEmail) is not null)
        {
            return;
        }
 
        var admin = new ApplicationUser
        {
            UserName = DevAdminEmail,
            Email = DevAdminEmail,
            EmailConfirmed = true, // pre-confirmed: RequireConfirmedEmail=true would otherwise block dev login
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
 
        var result = await userManager.CreateAsync(admin, DevAdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}