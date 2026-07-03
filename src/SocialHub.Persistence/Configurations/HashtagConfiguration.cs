using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Persistence.Configurations;
 
public class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.ToTable("hashtags");
        builder.HasKey(h => h.Id);
 
        builder.Property(h => h.Tag)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(h => h.NormalizedTag)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(h => h.CreatedAtUtc)
            .IsRequired();
 
        builder.Property(h => h.UsageCount)
            .HasDefaultValue(0);
 
        // 2.5 Indexes: uniqueness for lookups/creation checks, plus a
        // chronological index for "recently added" style queries later.
        builder.HasIndex(h => h.NormalizedTag).IsUnique();
        builder.HasIndex(h => h.CreatedAtUtc);
    }
}