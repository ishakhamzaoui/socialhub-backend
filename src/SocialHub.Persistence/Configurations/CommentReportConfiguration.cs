using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Persistence.Configurations;
 
public class CommentReportConfiguration : IEntityTypeConfiguration<CommentReport>
{
    public void Configure(EntityTypeBuilder<CommentReport> builder)
    {
        builder.ToTable("comment_reports");
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.Reason).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(r => r.Details).HasMaxLength(1000);
 
        // ExistsAsync's duplicate-report check. Deliberately NOT unique at
        // the DB level (see this script's header) — application-level
        // check only for this minimal Phase 7 shape.
        builder.HasIndex(r => new { r.CommentId, r.ReporterId });
    }
}