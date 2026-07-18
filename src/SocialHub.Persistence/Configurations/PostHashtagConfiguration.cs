using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostHashtagConfiguration : IEntityTypeConfiguration<PostHashtag>
{
    public void Configure(EntityTypeBuilder<PostHashtag> builder)
    {
        builder.ToTable("post_hashtags");
        builder.HasKey(h => h.Id);
 
        // One row per (post, hashtag) pair — Post.AddHashtag also guards
        // against duplicates in-memory, this is the DB-level backstop.
        builder.HasIndex(h => new { h.PostId, h.HashtagId }).IsUnique();
        builder.HasIndex(h => h.HashtagId);
    }
}