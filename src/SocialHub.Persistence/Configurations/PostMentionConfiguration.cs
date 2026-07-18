using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostMentionConfiguration : IEntityTypeConfiguration<PostMention>
{
    public void Configure(EntityTypeBuilder<PostMention> builder)
    {
        builder.ToTable("post_mentions");
        builder.HasKey(m => m.Id);
 
        builder.HasIndex(m => new { m.PostId, m.MentionedUserId }).IsUnique();
        builder.HasIndex(m => m.MentionedUserId);
    }
}