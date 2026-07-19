using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>Roadmap 7.2. Replies to a specific parent comment, paginated. Unlimited nesting depth in the data model (confirmed decision) — a reply can itself have replies, fetched via the same query with the reply's own Id as ParentCommentId.</summary>
public sealed record GetCommentRepliesQuery(Guid ParentCommentId, int Page = 1, int PageSize = 20) : IQuery<PagedCommentListDto>, IRequireAuthorization;