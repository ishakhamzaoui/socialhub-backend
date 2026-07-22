using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.ToTable("post_reactions");
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
 
        // Database-level enforcement of "one reaction per user per post" —
        // same reasoning as CommentReactionConfiguration's unique index.
        builder.HasIndex(r => new { r.PostId, r.UserId }).IsUnique();
 
        // GetCountsByTypeAsync's aggregation query.
        builder.HasIndex(r => new { r.PostId, r.Type });
    }
}