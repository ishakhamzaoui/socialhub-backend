using SocialHub.Domain.Common;
using SocialHub.Domain.Comments.Events;
 
namespace SocialHub.Domain.Comments;
 
/// <summary>
/// Roadmap 7.1-7.4, 7.7's aggregate root. AuthorId/PostId are bare Guids
/// (ApplicationUser.Id / Post.Id) — same pattern as Post.AuthorId; Domain
/// cannot reference SocialHub.Identity, and there is no DB-level FK back to
/// Post from here any more than Post.OriginalPostId has one back to itself
/// (resolved at the Application layer via IPostRepository when needed).
///
/// Confirmed decision (Phase 7 kickoff): Comment has NO visibility concept
/// of its own — it inherits whatever the parent Post's visibility resolves
/// to via the existing PostAccessPolicy (Phase 6). No CommentVisibility
/// enum exists anywhere in this codebase.
///
/// ParentCommentId is a self-reference for nested replies (confirmed:
/// unrestricted depth in the data model; any UI-facing depth cap is a
/// presentation concern, not enforced here).
///
/// This is its OWN aggregate root — NOT an owned child of Post, unlike
/// PostMedia/PostHashtag/PostMention. Reasoning (see script 42's header):
/// Comment needs independent paginated queries (top-level comments for a
/// post, replies for a parent comment) that don't fit the
/// owned-collection-reached-via-Include shape Post's children use.
///
/// Deletion is SOFT (flagged assumption, script 42 header item 8): unlike
/// Post.MarkDeleted() (paired with an actual repository.Remove), a
/// Comment's row survives deletion with Content tombstoned to null, so
/// replies underneath it in an unbounded-depth tree are never orphaned.
/// </summary>
public sealed class Comment : BaseEntity, IAggregateRoot
{
    private readonly List<CommentMention> _mentions = new();
 
    private Comment()
    {
        // Reserved for EF Core materialization.
    }
 
    private Comment(Guid id, Guid postId, Guid authorId, Guid? parentCommentId, string content)
        : base(id)
    {
        PostId = postId;
        AuthorId = authorId;
        ParentCommentId = parentCommentId;
        Content = content;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid PostId { get; private set; }
 
    public Guid AuthorId { get; private set; }
 
    /// <summary>Null for a top-level comment on the post; set for a reply. Self-referencing, unrestricted depth (confirmed decision).</summary>
    public Guid? ParentCommentId { get; private set; }
 
    /// <summary>
    /// Nullable at the Domain level, same reasoning family as
    /// Post.Content being nullable — a soft-deleted comment tombstones
    /// this to null. A live comment always has non-empty Content; that
    /// invariant is enforced in Create/UpdateContent below, not by the
    /// type system, because the deleted state legitimately has none.
    /// </summary>
    public string? Content { get; private set; }
 
    public bool IsPinned { get; private set; }
 
    public bool IsDeleted { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public DateTime? UpdatedAtUtc { get; private set; }
 
    public DateTime? DeletedAtUtc { get; private set; }
 
    public IReadOnlyCollection<CommentMention> Mentions => _mentions.AsReadOnly();
 
    public static Comment Create(Guid postId, Guid authorId, string content, Guid? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Comment content cannot be empty.", nameof(content));
        }
 
        var comment = new Comment(Guid.NewGuid(), postId, authorId, parentCommentId, content);
        comment.AddDomainEvent(new CommentAddedEvent(comment.Id, postId, authorId, parentCommentId));
        return comment;
    }
 
    public void UpdateContent(string content)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted comment cannot be edited.");
        }
 
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Comment content cannot be empty.", nameof(content));
        }
 
        Content = content;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>
    /// "At most one pinned comment per post" (mirroring Post's "at most
    /// one pinned post per author") is enforced at the Application layer,
    /// not here — a single Comment can't know about its post's other
    /// comments. Who is ALLOWED to pin (post author only, vs. also the
    /// comment's own author) is also an Application/handler-level
    /// decision — see PinCommentCommandHandler when it's added.
    /// </summary>
    public void Pin()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted comment cannot be pinned.");
        }
 
        IsPinned = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void Unpin()
    {
        IsPinned = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>Same dedup / no-self-mention guard as Post.AddMention.</summary>
    public void AddMention(Guid mentionedUserId)
    {
        if (mentionedUserId == AuthorId || _mentions.Any(m => m.MentionedUserId == mentionedUserId))
        {
            return;
        }
 
        _mentions.Add(CommentMention.Create(Id, mentionedUserId));
    }
 
    /// <summary>
    /// Soft delete (flagged assumption — see this file's header and script
    /// 42's header item 8). Tombstones Content, clears IsPinned, keeps the
    /// row (and its Id) so any replies underneath it in the thread are not
    /// orphaned. Unlike Post.MarkDeleted(), the caller does NOT also call
    /// repository.Remove() — the row is meant to persist in its deleted
    /// state.
    /// </summary>
    public void MarkDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Comment is already deleted.");
        }
 
        IsDeleted = true;
        IsPinned = false;
        Content = null;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new CommentDeletedEvent(Id, PostId, AuthorId));
    }
}