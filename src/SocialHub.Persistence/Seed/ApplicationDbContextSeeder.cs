using Microsoft.EntityFrameworkCore;
using SocialHub.Domain.Shared;
using SocialHub.Persistence.Context;
 
namespace SocialHub.Persistence.Seed;
 
public static class ApplicationDbContextSeeder
{
    private static readonly string[] DefaultHashtags =
    {
        "welcome", "introduction", "general", "announcements", "help"
    };
 
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Hashtags.AnyAsync(cancellationToken))
        {
            return;
        }
 
        var hashtags = DefaultHashtags.Select(Hashtag.Create).ToList();
        await context.Hashtags.AddRangeAsync(hashtags, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}