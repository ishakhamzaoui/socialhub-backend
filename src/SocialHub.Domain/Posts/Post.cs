using SocialHub.Domain.Common;
using SocialHub.Domain.Posts.Events;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.1-6.13's core aggregate. AuthorId is a bare Guid
/// (ApplicationUser.Id) — same pattern as MediaAsset.OwnerId /
/// UserProfile.UserId; Domain cannot reference SocialHub.Identity.
///
/// Owns PostMedia/PostHashtag/PostMention as child collections (confirmed
/// Phase 6 decision — see script 31's header comment for the reasoning).
/// Repost is NOT part of this aggregate; it's the separate PostRepost
/// aggregate in this same file's sibling class, added because a repost
/// relates two independent things (a user, someone else's post) rather than
/// being an owned part of one post.
///
/// Content is nullable at the Domain level on purpose: the "must have
/// Content or at least one attached MediaAsset" rule can't be checked until
/// after AttachMedia has run (media is attached post-construction, mirroring
/// the avatar/cover upload-then-attach flow) — that validation lives in the
/// Application-layer command handler, not here.
/// </summary>
public sealed class Post : BaseEntity, IAggregateRoot
{
    private readonly List<PostMedia> _media = new();
    private readonly List<PostHashtag> _hashtags = new();
    private readonly List<PostMention> _mentions = new();
 
    private Post()
    {
        // Reserved for EF Core materialization.
    }
 
    private Post(
        Guid id,
        Guid authorId,
        string? content,
        PostType type,
        Guid? originalPostId,
        PostVisibility visibility,
        PostStatus status,
        DateTime? scheduledForUtc)
        : base(id)
    {
        AuthorId = authorId;
        Content = content;
        Type = type;
        OriginalPostId = originalPostId;
        Visibility = visibility;
        Status = status;
        ScheduledForUtc = scheduledForUtc;
        PublishedAtUtc = status == PostStatus.Published ? DateTime.UtcNow : null;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid AuthorId { get; private set; }
 
    public string? Content { get; private set; }
 
    public PostType Type { get; private set; }
 
    /// <summary>Only set when Type == Quote. Points at the quoted Post.Id.</summary>
    public Guid? OriginalPostId { get; private set; }
 
    public PostVisibility Visibility { get; private set; }
 
    public PostStatus Status { get; private set; }
 
    public DateTime? ScheduledForUtc { get; private set; }
 
    public DateTime? PublishedAtUtc { get; private set; }
 
    public bool IsPinned { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public DateTime? UpdatedAtUtc { get; private set; }
 
    public IReadOnlyCollection<PostMedia> Media => _media.AsReadOnly();
 
    public IReadOnlyCollection<PostHashtag> Hashtags => _hashtags.AsReadOnly();
 
    public IReadOnlyCollection<PostMention> Mentions => _mentions.AsReadOnly();
 
    public static Post CreateDraft(Guid authorId, string? content, PostVisibility visibility, PostType type = PostType.Original, Guid? originalPostId = null)
    {
        ValidateType(type, originalPostId);
        var post = new Post(Guid.NewGuid(), authorId, content, type, originalPostId, visibility, PostStatus.Draft, null);
        post.AddDomainEvent(new PostCreatedEvent(post.Id, authorId));
        return post;
    }
 
    public static Post CreateScheduled(Guid authorId, string? content, PostVisibility visibility, DateTime scheduledForUtc, PostType type = PostType.Original, Guid? originalPostId = null)
    {
        if (scheduledForUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledForUtc));
        }
 
        ValidateType(type, originalPostId);
        var post = new Post(Guid.NewGuid(), authorId, content, type, originalPostId, visibility, PostStatus.Scheduled, scheduledForUtc);
        post.AddDomainEvent(new PostCreatedEvent(post.Id, authorId));
        return post;
    }
 
    public static Post CreatePublished(Guid authorId, string? content, PostVisibility visibility, PostType type = PostType.Original, Guid? originalPostId = null)
    {
        ValidateType(type, originalPostId);
        var post = new Post(Guid.NewGuid(), authorId, content, type, originalPostId, visibility, PostStatus.Published, null);
        post.AddDomainEvent(new PostCreatedEvent(post.Id, authorId));
        return post;
    }
 
    private static void ValidateType(PostType type, Guid? originalPostId)
    {
        if (type == PostType.Quote && originalPostId is null)
        {
            throw new ArgumentException("A quote post must reference an original post.", nameof(originalPostId));
        }
 
        if (type == PostType.Original && originalPostId is not null)
        {
            throw new ArgumentException("An original post cannot reference another post.", nameof(originalPostId));
        }
    }
 
    public void UpdateContent(string? content)
    {
        if (Status == PostStatus.Archived)
        {
            throw new InvalidOperationException("An archived post cannot be edited.");
        }
 
        Content = content;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void ChangeVisibility(PostVisibility visibility)
    {
        Visibility = visibility;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>Draft or Scheduled -> Published. Covers both "publish now" from a draft and the scheduled-post background job's transition.</summary>
    public void Publish()
    {
        if (Status is not (PostStatus.Draft or PostStatus.Scheduled))
        {
            throw new InvalidOperationException($"Cannot publish a post in {Status} status.");
        }
 
        Status = PostStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
        ScheduledForUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>Draft -> Scheduled, or re-schedules an already-Scheduled post.</summary>
    public void Schedule(DateTime scheduledForUtc)
    {
        if (Status is not (PostStatus.Draft or PostStatus.Scheduled))
        {
            throw new InvalidOperationException($"Cannot schedule a post in {Status} status.");
        }
 
        if (scheduledForUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledForUtc));
        }
 
        Status = PostStatus.Scheduled;
        ScheduledForUtc = scheduledForUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void Archive()
    {
        if (Status == PostStatus.Archived)
        {
            throw new InvalidOperationException("Post is already archived.");
        }
 
        Status = PostStatus.Archived;
        IsPinned = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>Not exposed via any Phase 6 endpoint (no roadmap step calls for it) — provided so Archived isn't a hard dead end if a later phase needs it.</summary>
    public void Restore()
    {
        if (Status != PostStatus.Archived)
        {
            throw new InvalidOperationException("Only an archived post can be restored.");
        }
 
        Status = PostStatus.Published;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    /// <summary>"At most one pinned post per author" is enforced at the Application layer (unpin any prior pinned post before pinning this one) — a single Post can't know about its author's other posts.</summary>
    public void Pin()
    {
        if (Status is not PostStatus.Published)
        {
            throw new InvalidOperationException("Only a published post can be pinned.");
        }
 
        IsPinned = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void Unpin()
    {
        IsPinned = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void AttachMedia(Guid mediaAssetId, int order)
    {
        _media.Add(PostMedia.Create(Id, mediaAssetId, order));
        UpdatedAtUtc = DateTime.UtcNow;
    }
 
    public void AddHashtag(Guid hashtagId)
    {
        if (_hashtags.Any(h => h.HashtagId == hashtagId))
        {
            return;
        }
 
        _hashtags.Add(PostHashtag.Create(Id, hashtagId));
    }
 
    public void AddMention(Guid mentionedUserId)
    {
        if (mentionedUserId == AuthorId || _mentions.Any(m => m.MentionedUserId == mentionedUserId))
        {
            return;
        }
 
        _mentions.Add(PostMention.Create(Id, mentionedUserId));
    }
 
    /// <summary>Raises PostDeletedEvent; caller still removes the row via IRepository.Remove separately — same pattern as MediaAsset.MarkDeleted() / Follow.MarkUnfollowed().</summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new PostDeletedEvent(Id, AuthorId));
    }
}