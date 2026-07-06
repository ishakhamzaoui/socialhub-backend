namespace SocialHub.Domain.Media;
 
/// <summary>
/// Maps 1:1 to the top-level upload folders defined in spec §22
/// (users/posts/messages/communities — "temp" is deliberately not a member
/// here; see MediaAsset's class remarks for why). Determines the on-disk
/// destination folder and, indirectly, which future feature phase owns the
/// file (Avatar/Cover uploads -> User in Phase 5, Post attachments -> Post
/// in Phase 6, message attachments -> Message in Phase 10, community media
/// -> Community in Phase 12). Avatar and Cover both live under "users/";
/// nothing in Phase 4 needs to distinguish them further at the storage
/// layer, but callers are free to track that distinction themselves (e.g.
/// via ApplicationUser.AvatarMediaId vs. CoverMediaId columns added in
/// Phase 5) without any change here.
/// </summary>
public enum MediaCategory
{
    User,
    Post,
    Message,
    Community
}