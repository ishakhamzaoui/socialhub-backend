using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users;
 
/// <summary>
/// Roadmap 5.7. BlockerId has blocked BlockedId. No domain events are raised
/// here (deliberate scope decision for Phase 5 — nothing currently consumes
/// a "user blocked" notification; add one later if a phase needs it, this
/// isn't load-bearing for anything today).
///
/// Blocking is a harder relational action than muting: creating a block
/// also removes any existing Follow row between the two users in either
/// direction (handled by the Application-layer BlockUserCommandHandler, not
/// here, since that requires IFollowRepository).
/// </summary>
public sealed class UserBlock : BaseEntity, IAggregateRoot
{
    private UserBlock()
    {
        // Reserved for EF Core materialization.
    }
 
    private UserBlock(Guid id, Guid blockerId, Guid blockedId)
        : base(id)
    {
        BlockerId = blockerId;
        BlockedId = blockedId;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid BlockerId { get; private set; }
 
    public Guid BlockedId { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static UserBlock Create(Guid blockerId, Guid blockedId)
    {
        if (blockerId == blockedId)
        {
            throw new ArgumentException("A user cannot block themselves.", nameof(blockedId));
        }
 
        return new UserBlock(Guid.NewGuid(), blockerId, blockedId);
    }
}