using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Comments;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 6.1-6.13: Posts. Every action requires authentication
/// ([Authorize] here, plus IRequireAuthorization on every command/query
/// itself, per spec §2's "authorization checks occur in the Application
/// layer via pipeline behaviors, not solely in controllers").
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class PostsController : ControllerBase
{
    private readonly ISender _sender;
 
    public PostsController(ISender sender)
    {
        _sender = sender;
    }
 
    /// <summary>Roadmap 6.1, 6.5, 6.6/6.7, 6.10, 6.12, 6.13.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(
            request.Content,
            request.Visibility,
            request.Type,
            request.OriginalPostId,
            request.DesiredStatus,
            request.ScheduledForUtc,
            request.MediaAssetIds,
            request.HashtagTags,
            request.MentionedUserIds);
 
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Always the authenticated caller's own posts (roadmap 6.6/6.7/6.8 management view) — see GetMyPostsQuery's remarks for why this is deliberately not a general per-user endpoint.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine([FromQuery] PostStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetMyPostsQuery(status, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.4 — visibility (including the block check) resolved via PostAccessPolicy.</summary>
    [HttpGet("{postId:guid}")]
    public async Task<IActionResult> Get(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPostQuery(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>
    /// Phase 7 addition. Top-level comments only (ParentCommentId null) —
    /// use CommentsController's GetReplies for a given comment's replies.
    /// Access/visibility is resolved by GetPostCommentsQuery reusing
    /// PostAccessPolicy exactly the way Get(postId) above does.
    /// </summary>
    [HttpGet("{postId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPostCommentsQuery(postId, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.2. Content/Visibility only — see UpdatePostCommand's remarks.</summary>
    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> Update(Guid postId, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdatePostCommand(postId, request.Content, request.Visibility), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.3.</summary>
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> Delete(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeletePostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.6/6.7 — publish a Draft (or an already-Scheduled post) now.</summary>
    [HttpPost("{postId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishPostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.7.</summary>
    [HttpPut("{postId:guid}/schedule")]
    public async Task<IActionResult> Schedule(Guid postId, [FromBody] SchedulePostRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SchedulePostCommand(postId, request.ScheduledForUtc), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.8.</summary>
    [HttpPost("{postId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ArchivePostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.9.</summary>
    [HttpPost("{postId:guid}/pin")]
    public async Task<IActionResult> Pin(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PinPostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{postId:guid}/pin")]
    public async Task<IActionResult> Unpin(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnpinPostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 6.11. postId here is the post being reposted (RepostCommand's OriginalPostId).</summary>
    [HttpPost("{postId:guid}/repost")]
    public async Task<IActionResult> Repost(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RepostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{postId:guid}/repost")]
    public async Task<IActionResult> UndoRepost(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UndoRepostCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>
    /// Added in script 53 (Phase 8) — Posts previously had no like/reaction
    /// capability at all (see PostReaction's remarks). Mirrors
    /// CommentsController's React/RemoveReaction actions exactly.
    /// </summary>
    [HttpPost("{postId:guid}/reactions")]
    public async Task<IActionResult> React(Guid postId, [FromBody] ReactToPostRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReactToPostCommand(postId, request.Type), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{postId:guid}/reactions")]
    public async Task<IActionResult> RemoveReaction(Guid postId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemovePostReactionCommand(postId), cancellationToken);
        return result.ToActionResult(this);
    }
}
 
public sealed class CreatePostRequest
{
    public string? Content { get; set; }
    public PostVisibility Visibility { get; set; }
    public PostType Type { get; set; } = PostType.Original;
    public Guid? OriginalPostId { get; set; }
    public PostStatus DesiredStatus { get; set; } = PostStatus.Published;
    public DateTime? ScheduledForUtc { get; set; }
    public IReadOnlyList<Guid>? MediaAssetIds { get; set; }
    public IReadOnlyList<string>? HashtagTags { get; set; }
    public IReadOnlyList<Guid>? MentionedUserIds { get; set; }
}
 
public sealed class UpdatePostRequest
{
    public string? Content { get; set; }
    public PostVisibility Visibility { get; set; }
}
 
public sealed class SchedulePostRequest
{
    public DateTime ScheduledForUtc { get; set; }
}
 
public sealed class ReactToPostRequest
{
    public ReactionType Type { get; set; }
}