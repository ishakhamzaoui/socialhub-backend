using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Persistence.Configurations;
 
public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("post_media");
        builder.HasKey(m => m.Id);
 
        builder.Property(m => m.Order).IsRequired();
 
        builder.HasIndex(m => m.PostId);
        builder.HasIndex(m => m.MediaAssetId);
    }
}