using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialHub.Domain.Shared;
using SocialHub.Identity.Models;
using SocialHub.Identity.Permissions;
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
 
    // Starter role -> permission mapping. Later phases extend Permissions.All
    // and should extend this map alongside it.
    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        ["Admin"] = Permissions.All.ToArray(),
        ["Moderator"] = new[]
        {
            Permissions.Users.View,
            Permissions.Roles.View,
            Permissions.System.ViewAuditLog
        },
        ["User"] = Array.Empty<string>()
    };
 
    // DEV-ONLY credential, same pattern as the Postgres dev password from
    // Phase 0 — never used in production. Phase 3's real registration flow
    // is how production admins actually get created.
    private const string DevAdminEmail = "admin@socialhub.local";
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
            var role = await roleManager.FindByNameAsync(name);
            if (role is null)
            {
                role = new ApplicationRole(name, description);
                await roleManager.CreateAsync(role);
            }
 
            if (!RolePermissions.TryGetValue(name, out var permissions) || permissions.Length == 0)
            {
                continue;
            }
 
            var existingPermissionValues = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();
 
            foreach (var permission in permissions)
            {
                if (!existingPermissionValues.Contains(permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }
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