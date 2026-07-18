using SocialHub.Domain.Common;
 
namespace SocialHub.Domain.Posts;
 
/// <summary>
/// Roadmap 6.13. A single @mention parsed out of a post's content. Owned by
/// the Post aggregate, same rationale as PostMedia/PostHashtag.
/// MentionedUserId is a bare Guid (ApplicationUser.Id) — Domain cannot
/// reference SocialHub.Identity, same pattern as MediaAsset.OwnerId /
/// UserProfile.UserId. Parsing @handles out of Content into user IDs is an
/// Application-layer concern (needs a username lookup), not Domain's.
/// </summary>
public sealed class PostMention : BaseEntity
{
    private PostMention()
    {
        // Reserved for EF Core materialization.
    }
 
    private PostMention(Guid id, Guid postId, Guid mentionedUserId)
        : base(id)
    {
        PostId = postId;
        MentionedUserId = mentionedUserId;
    }
 
    public Guid PostId { get; private set; }
 
    public Guid MentionedUserId { get; private set; }
 
    internal static PostMention Create(Guid postId, Guid mentionedUserId) =>
        new(Guid.NewGuid(), postId, mentionedUserId);
}