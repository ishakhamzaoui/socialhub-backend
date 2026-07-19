using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Persistence.Configurations;
 
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(c => c.Id);
 
        builder.Property(c => c.Content).HasMaxLength(2000);
        builder.Property(c => c.CreatedAtUtc).IsRequired();
 
        // Top-level comments for a post (GetTopLevelForPostAsync).
        builder.HasIndex(c => new { c.PostId, c.ParentCommentId });
 
        // Replies to a specific parent (GetRepliesAsync / GetReplyCountAsync).
        builder.HasIndex(c => c.ParentCommentId);
 
        // Author-scoped edit/delete lookups.
        builder.HasIndex(c => c.AuthorId);
 
        // "At most one pinned comment per post" lookup (GetPinnedCommentForPostAsync).
        builder.HasIndex(c => new { c.PostId, c.IsPinned });
 
        // Self-referencing ParentCommentId (confirmed decision: unrestricted
        // depth). No navigation property back to the parent Comment exists
        // on purpose — same bare-Guid-no-FK-navigation pattern
        // Post.OriginalPostId already uses; ParentCommentId still gets a
        // real FK constraint here (unlike OriginalPostId, which points at a
        // different aggregate type entirely), but Restrict rather than
        // Cascade, since a soft-deleted comment's replies must survive
        // independently of anything happening to the row's own FK target.
        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
 
        // Owned child collection (Mentions) — same EF backing-field
        // navigation pattern PostConfiguration established in Phase 6.
        builder.HasMany(c => c.Mentions)
            .WithOne()
            .HasForeignKey(m => m.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Mentions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_mentions");
    }
}