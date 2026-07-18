using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.5. A single ordered media attachment on a post. Owned by the
/// Post aggregate — no independent repository; only Post.AttachMedia adds
/// rows, and they're persisted/removed as part of saving the owning Post.
/// MediaAssetId is a bare Guid reference to MediaAsset.Id (same
/// sibling-namespace-no-dependency pattern UserProfile.AvatarMediaId
/// already uses for Domain.Media) — the actual MediaAsset is resolved via
/// IMediaAssetRepository at the Application layer when needed.
/// </summary>
public sealed class PostMedia : BaseEntity
{
    private PostMedia()
    {
        // Reserved for EF Core materialization.
    }
 
    private PostMedia(Guid id, Guid postId, Guid mediaAssetId, int order)
        : base(id)
    {
        PostId = postId;
        MediaAssetId = mediaAssetId;
        Order = order;
    }
 
    public Guid PostId { get; private set; }
 
    public Guid MediaAssetId { get; private set; }
 
    public int Order { get; private set; }
 
    internal static PostMedia Create(Guid postId, Guid mediaAssetId, int order) =>
        new(Guid.NewGuid(), postId, mediaAssetId, order);
}