using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostRepostConfiguration : IEntityTypeConfiguration<PostRepost>
{
    public void Configure(EntityTypeBuilder<PostRepost> builder)
    {
        builder.ToTable("post_reposts");
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.CreatedAtUtc).IsRequired();
 
        // A user can only repost a given post once.
        builder.HasIndex(r => new { r.UserId, r.OriginalPostId }).IsUnique();
 
        // Repost-count queries.
        builder.HasIndex(r => r.OriginalPostId);
    }
}