using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Not one of the roadmap's explicit 7.1-7.8 line items — added as a
/// natural completion symmetrical with Posts/GetPostQuery (deep-linking to
/// a single comment, or refreshing state after a mutation elsewhere).
/// Flagged here rather than silently introduced.
/// </summary>
public sealed record GetCommentQuery(Guid CommentId) : IQuery<CommentDto>, IRequireAuthorization;