using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Persistence.Configurations;
 
public class CommentMentionConfiguration : IEntityTypeConfiguration<CommentMention>
{
    public void Configure(EntityTypeBuilder<CommentMention> builder)
    {
        builder.ToTable("comment_mentions");
        builder.HasKey(m => m.Id);
 
        builder.HasIndex(m => new { m.CommentId, m.MentionedUserId }).IsUnique();
        builder.HasIndex(m => m.MentionedUserId);
    }
}