using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Deliberately minimal (confirmed Phase 7 scope decision — see
/// CommentReport.cs's remarks): just enough to stop the same user from
/// filing a duplicate report on the same comment. No queue/status query
/// here — Phase 14 (Moderation) owns designing how reports get reviewed.
/// </summary>
public interface ICommentReportRepository : IRepository<CommentReport, Guid>
{
    Task<bool> ExistsAsync(Guid commentId, Guid reporterId, CancellationToken cancellationToken = default);
}