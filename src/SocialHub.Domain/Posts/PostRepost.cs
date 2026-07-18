using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.11. Confirmed decision: a repost has no content of its own, so
/// it is NOT a Post row — it's this separate, lightweight aggregate: "UserId
/// reposted OriginalPostId, at CreatedAtUtc." Same shape as Domain.Users.Follow
/// (relates two independent things), not an owned child of Post. No domain
/// event raised yet — no Phase 6 consumer exists (matches the UserMute
/// precedent of not adding events preemptively); Phase 11 can add one when
/// repost notifications are built.
/// </summary>
public sealed class PostRepost : BaseEntity, IAggregateRoot
{
    private PostRepost()
    {
        // Reserved for EF Core materialization.
    }
 
    private PostRepost(Guid id, Guid userId, Guid originalPostId)
        : base(id)
    {
        UserId = userId;
        OriginalPostId = originalPostId;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid UserId { get; private set; }
 
    public Guid OriginalPostId { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static PostRepost Create(Guid userId, Guid originalPostId) =>
        new(Guid.NewGuid(), userId, originalPostId);
}