using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>Roadmap 7.1. Top-level comments only (ParentCommentId null) — use GetCommentRepliesQuery for a given comment's replies.</summary>
public sealed record GetPostCommentsQuery(Guid PostId, int Page = 1, int PageSize = 20) : IQuery<PagedCommentListDto>, IRequireAuthorization;