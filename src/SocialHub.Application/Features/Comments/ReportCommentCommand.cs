using SocialHub.Application.Common.Behaviors;
using SocialHub.Application.Common.Messaging;
using SocialHub.Domain.Comments;
 
namespace SocialHub.Application.Features.Comments;
 
/// <summary>
/// Roadmap 7.8. Flagged assumption (Phase 7 kickoff, item 7): a minimal,
/// comment-specific report record — no queue/status/workflow. Phase 14
/// (Moderation) owns the general cross-entity Reports system.
/// </summary>
public sealed record ReportCommentCommand(Guid CommentId, CommentReportReason Reason, string? Details) : ICommand, IRequireAuthorization, ITransactionalRequest;