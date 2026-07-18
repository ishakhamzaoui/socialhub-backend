using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
/// <summary>
/// Always scoped to the authenticated caller's own posts (their drafts,
/// scheduled queue, published posts, or archive — optionally filtered to
/// one status). NOT a general "get any user's posts" endpoint — see this
/// script's header for why that's a deliberate boundary, not an oversight.
/// </summary>
public sealed record GetMyPostsQuery(PostStatus? Status, int Page, int PageSize) : IQuery<PagedPostListDto>, IRequireAuthorization;