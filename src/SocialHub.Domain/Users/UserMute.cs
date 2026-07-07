using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Users;
 
/// <summary>
/// Roadmap 5.8. MuterId has muted MutedId. Deliberately has NO effect on
/// follow-graph queries (unlike UserBlock) — muting only suppresses content
/// in a feed, which doesn't exist until Phase 8. This row is persisted now
/// so Phase 8's feed queries have something to read; it is not yet consumed
/// by anything as of Phase 5. See the Phase 5 context doc for why this is
/// treated differently from blocking despite the roadmap listing them
/// adjacently.
/// </summary>
public sealed class UserMute : BaseEntity, IAggregateRoot
{
    private UserMute()
    {
        // Reserved for EF Core materialization.
    }
 
    private UserMute(Guid id, Guid muterId, Guid mutedId)
        : base(id)
    {
        MuterId = muterId;
        MutedId = mutedId;
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public Guid MuterId { get; private set; }
 
    public Guid MutedId { get; private set; }
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public static UserMute Create(Guid muterId, Guid mutedId)
    {
        if (muterId == mutedId)
        {
            throw new ArgumentException("A user cannot mute themselves.", nameof(mutedId));
        }
 
        return new UserMute(Guid.NewGuid(), muterId, mutedId);
    }
}