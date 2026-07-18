using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.12. Join row linking a Post to an existing Domain.Shared.Hashtag
/// (Phase 2). Owned by the Post aggregate, same rationale as PostMedia —
/// only Post.AddHashtag creates rows. HashtagId is a bare Guid; resolving/
/// creating the Hashtag row itself (get-or-create by normalized tag,
/// incrementing UsageCount) is the Application-layer handler's job, via
/// IHashtagRepository, before calling Post.AddHashtag.
/// </summary>
public sealed class PostHashtag : BaseEntity
{
    private PostHashtag()
    {
        // Reserved for EF Core materialization.
    }
 
    private PostHashtag(Guid id, Guid postId, Guid hashtagId)
        : base(id)
    {
        PostId = postId;
        HashtagId = hashtagId;
    }
 
    public Guid PostId { get; private set; }
 
    public Guid HashtagId { get; private set; }
 
    internal static PostHashtag Create(Guid postId, Guid hashtagId) =>
        new(Guid.NewGuid(), postId, hashtagId);
}