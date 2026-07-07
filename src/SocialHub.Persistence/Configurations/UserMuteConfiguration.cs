using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Users;
 
namespace SocialHub.Persistence.Configurations;
 
public class UserMuteConfiguration : IEntityTypeConfiguration<UserMute>
{
    public void Configure(EntityTypeBuilder<UserMute> builder)
    {
        builder.ToTable("user_mutes");
        builder.HasKey(m => m.Id);
 
        builder.Property(m => m.CreatedAtUtc).IsRequired();
 
        builder.HasIndex(m => new { m.MuterId, m.MutedId }).IsUnique();
    }
}