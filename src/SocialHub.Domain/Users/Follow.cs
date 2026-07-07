using SocialHub.Domain.Common;
using SocialHub.Domain.Users.Events;
 
namespace SocialHub.Domain.Users;
 
/// <summary>
/// Roadmap 5.10-5.13. A directed edge in the social graph: FollowerId
/// follows FollowingId. Uniqueness on (FollowerId, FollowingId) is enforced
/// at the Persistence layer (unique index) — see FollowConfiguration.
/// </summary>
public sealed class Follow : BaseEntity, IAggregateRoot
{
    private Follow()
    {
        // Reserved for EF Core materialization.
    }
 
    private Follow(Guid id, Guid followerId, Guid followingId)
        : base(id)
    {
        FollowerId = followerId;
        FollowingId = followingId;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid FollowerId { get; private set; }
 
    public Guid FollowingId { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static Follow Create(Guid followerId, Guid followingId)
    {
        if (followerId == followingId)
        {
            throw new ArgumentException("A user cannot follow themselves.", nameof(followingId));
        }
 
        var follow = new Follow(Guid.NewGuid(), followerId, followingId);
        follow.AddDomainEvent(new UserFollowedEvent(followerId, followingId));
        return follow;
    }
 
    /// <summary>Raises UserUnfollowedEvent. Caller still removes the row via IRepository.Remove separately — same pattern as MediaAsset.MarkDeleted().</summary>
    public void MarkUnfollowed()
    {
        AddDomainEvent(new UserUnfollowedEvent(FollowerId, FollowingId));
    }
}