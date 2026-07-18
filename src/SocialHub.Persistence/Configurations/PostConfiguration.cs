using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.Content).HasMaxLength(5000);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
 
        // Author's own post list (drafts/scheduled/published/archived filter).
        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => new { p.AuthorId, p.Status });
 
        // Scheduled-post publishing background job's due-post query (6.7).
        builder.HasIndex(p => p.ScheduledForUtc);
 
        // Quote lookups / repost-count-adjacent queries.
        builder.HasIndex(p => p.OriginalPostId);
 
        // Owned child collections (script 31's design decision) — each is a
        // real table with its own FK to PostId, backed by Post's private
        // List<T> fields rather than a public settable collection property.
        builder.HasMany(p => p.Media)
            .WithOne()
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Media)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_media");
 
        builder.HasMany(p => p.Hashtags)
            .WithOne()
            .HasForeignKey(h => h.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Hashtags)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_hashtags");
 
        builder.HasMany(p => p.Mentions)
            .WithOne()
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Mentions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_mentions");
    }
}