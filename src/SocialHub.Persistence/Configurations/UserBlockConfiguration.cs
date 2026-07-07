using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Configurations;
 
public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("user_blocks");
        builder.HasKey(b => b.Id);
 
        builder.Property(b => b.CreatedAtUtc).IsRequired();
 
        builder.HasIndex(b => new { b.BlockerId, b.BlockedId }).IsUnique();
        builder.HasIndex(b => b.BlockedId);
    }
}