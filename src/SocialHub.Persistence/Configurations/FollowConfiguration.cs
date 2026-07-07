using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Configurations;
 
public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows");
        builder.HasKey(f => f.Id);
 
        builder.Property(f => f.CreatedAtUtc).IsRequired();
 
        // Prevents duplicate follow rows; also the query path for
        // ExistsAsync/GetAsync (5.10-5.11).
        builder.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
 
        // Supports followers-list (5.12) queries filtered by FollowingId
        // independently of the composite index above.
        builder.HasIndex(f => f.FollowingId);
    }
}