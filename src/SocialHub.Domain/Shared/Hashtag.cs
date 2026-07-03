using SocialHub.Domain.Common;
using SocialHub.Domain.Shared.Events;
 
namespace SocialHub.Domain.Shared;
 
/// <summary>
/// A hashtag usable across features (Posts §15.4, Search §15.10). Deliberately
/// has no foreign keys yet — Post/Community associations are added by the
/// phases that introduce those aggregates.
/// </summary>
public sealed class Hashtag : BaseEntity, IAggregateRoot
{
    private Hashtag()
    {
        // Reserved for EF Core materialization.
    }
 
    private Hashtag(Guid id, string tag)
        : base(id)
    {
        Tag = tag;
        NormalizedTag = tag.ToUpperInvariant();
        CreatedAtUtc = DateTime.UtcNow;
    }
 
    public string Tag { get; private set; } = default!;
 
    public string NormalizedTag { get; private set; } = default!;
 
    public DateTime CreatedAtUtc { get; private set; }
 
    public int UsageCount { get; private set; }
 
    public static Hashtag Create(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Hashtag cannot be empty.", nameof(tag));
        }
 
        var normalized = tag.Trim().TrimStart('#');
        var hashtag = new Hashtag(Guid.NewGuid(), normalized);
        hashtag.AddDomainEvent(new HashtagCreatedEvent(hashtag.Id, hashtag.Tag));
 
        return hashtag;
    }
 
    public void IncrementUsage() => UsageCount++;
}