using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Comments;
using SocialHub.Domain.Comments;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 7.1-7.8: Comments &amp; Reactions. Every action requires
/// authentication ([Authorize] here, plus IRequireAuthorization on every
/// command/query itself — spec §2's "authorization checks occur in the
/// Application layer via pipeline behaviors, not solely in controllers").
///
/// Listing TOP-LEVEL comments for a post lives on PostsController instead
/// (GET /api/v1/posts/{postId}/comments) — see script 46's header for the
/// routing rationale.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class CommentsController : ControllerBase
{
    private readonly ISender _sender;
 
    public CommentsController(ISender sender)
    {
        _sender = sender;
    }
 
    /// <summary>Roadmap 7.1 (top-level, ParentCommentId null) / 7.2 (reply, ParentCommentId set) / 7.7 (mentions).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(request.PostId, request.ParentCommentId, request.Content ?? string.Empty, request.MentionedUserIds);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Not one of the roadmap's explicit line items — a single-comment getter, symmetrical with Posts/GetPostQuery. See GetCommentQuery's remarks.</summary>
    [HttpGet("{commentId:guid}")]
    public async Task<IActionResult> Get(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCommentQuery(commentId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.2. Replies to this comment, paginated.</summary>
    [HttpGet("{commentId:guid}/replies")]
    public async Task<IActionResult> GetReplies(Guid commentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetCommentRepliesQuery(commentId, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.3.</summary>
    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> Update(Guid commentId, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateCommentCommand(commentId, request.Content ?? string.Empty), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.3. Soft delete — see Comment.MarkDeleted()'s remarks.</summary>
    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCommentCommand(commentId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.4. Only the post's author may pin — see PinCommentCommandHandler's remarks (flagged assumption).</summary>
    [HttpPost("{commentId:guid}/pin")]
    public async Task<IActionResult> Pin(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PinCommentCommand(commentId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{commentId:guid}/pin")]
    public async Task<IActionResult> Unpin(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnpinCommentCommand(commentId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.5/7.6 — unified reaction mechanism (flagged assumption, see ReactToCommentCommand's remarks). Reacting again with a different type changes the existing reaction.</summary>
    [HttpPost("{commentId:guid}/reactions")]
    public async Task<IActionResult> React(Guid commentId, [FromBody] ReactToCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReactToCommentCommand(commentId, request.Type), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{commentId:guid}/reactions")]
    public async Task<IActionResult> RemoveReaction(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveCommentReactionCommand(commentId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 7.8. Minimal record only — see ReportCommentCommand's remarks (flagged assumption, Phase 14 owns the general moderation workflow).</summary>
    [HttpPost("{commentId:guid}/report")]
    public async Task<IActionResult> Report(Guid commentId, [FromBody] ReportCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReportCommentCommand(commentId, request.Reason, request.Details), cancellationToken);
        return result.ToActionResult(this);
    }
}
 
public sealed class CreateCommentRequest
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string? Content { get; set; }
    public IReadOnlyList<Guid>? MentionedUserIds { get; set; }
}
 
public sealed class UpdateCommentRequest
{
    public string? Content { get; set; }
}
 
public sealed class ReactToCommentRequest
{
    public ReactionType Type { get; set; }
}
 
public sealed class ReportCommentRequest
{
    public CommentReportReason Reason { get; set; }
    public string? Details { get; set; }
}