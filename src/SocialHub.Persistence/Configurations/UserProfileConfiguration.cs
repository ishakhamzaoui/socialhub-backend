using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Configurations;
 
public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.Location).HasMaxLength(100);
        builder.Property(p => p.Website).HasMaxLength(200);
        builder.Property(p => p.Language).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Theme).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
 
        // One profile per user (5.1) — created once at registration time.
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}