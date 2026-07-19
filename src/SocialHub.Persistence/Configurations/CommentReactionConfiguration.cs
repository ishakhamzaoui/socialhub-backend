using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Persistence.Configurations;
 
public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.ToTable("comment_reactions");
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
 
        // Database-level enforcement of "one reaction per user per comment"
        // — not just an application-layer check (same belt-and-braces
        // reasoning as PostMentionConfiguration's unique index).
        builder.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique();
 
        // GetCountsByTypeAsync's aggregation query.
        builder.HasIndex(r => new { r.CommentId, r.Type });
    }
}