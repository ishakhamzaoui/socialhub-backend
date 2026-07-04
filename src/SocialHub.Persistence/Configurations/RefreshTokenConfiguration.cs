using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Configurations;
 
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
 
        builder.Property(x => x.TokenHash)
            .HasMaxLength(512)
            .IsRequired();
 
        // Unique: a redeemed/rotated token hash being replayed must never
        // silently coexist with another live row.
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
 
        builder.Property(x => x.CreatedByIp).HasMaxLength(64);
        builder.Property(x => x.RevokedByIp).HasMaxLength(64);
    }
}