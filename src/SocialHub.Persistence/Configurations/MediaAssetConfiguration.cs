using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Media;
 
namespace SocialHub.Persistence.Configurations;
 
public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(m => m.Id);
 
        builder.Property(m => m.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(m => m.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(m => m.ThumbnailStoragePath).HasMaxLength(1024);
        builder.Property(m => m.MimeType).HasMaxLength(255).IsRequired();
 
        // Stored as strings for direct readability when inspecting rows by
        // hand during development — there is no volume/perf reason to prefer
        // int here at this stage.
        builder.Property(m => m.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
 
        builder.Property(m => m.CreatedAtUtc).IsRequired();
 
        // 4.3 Media metadata persistence — indexes for the query patterns
        // Phase 4 (list-by-owner) and later phases (e.g. Phase 5 avatar
        // lookup by owner+category) will actually run.
        builder.HasIndex(m => m.OwnerId);
        builder.HasIndex(m => new { m.OwnerId, m.Category });
    }
}