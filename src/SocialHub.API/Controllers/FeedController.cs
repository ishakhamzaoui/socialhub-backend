using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Feed;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 8.1-8.4 (8.5, Community feed, is explicitly deferred — no
/// Community domain exists until Phase 12; see the Phase 8 Advancement doc
/// entry). Injects ICurrentUserService directly, alongside ISender — every
/// other controller in this codebase only ever injects ISender and lets the
/// handler resolve the current user internally, but the feed queries need
/// RequesterId set on the query object itself before it reaches
/// CachingBehavior (see script 56's header for the full reasoning).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/feed")]
[Authorize]
public sealed class FeedController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;
 
    public FeedController(ISender sender, ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }
 
    /// <summary>Roadmap 8.1 — posts and reposts from users the requester follows.</summary>
    [HttpGet("following")]
    public async Task<IActionResult> Following([FromQuery] string? cursor, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var requesterId = await ResolveRequesterIdAsync();
        if (requesterId is null)
        {
            return Unauthorized();
        }
 
        var result = await _sender.Send(new GetFollowingFeedQuery(requesterId.Value, cursor, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 8.2 — the platform-wide public timeline.</summary>
    [HttpGet("chronological")]
    public async Task<IActionResult> Chronological([FromQuery] string? cursor, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var requesterId = await ResolveRequesterIdAsync();
        if (requesterId is null)
        {
            return Unauthorized();
        }
 
        var result = await _sender.Send(new GetChronologicalFeedQuery(requesterId.Value, cursor, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 8.3 — ranked by repost count within a recent window (confirmed "keep it simple" design).</summary>
    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] string? cursor, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var requesterId = await ResolveRequesterIdAsync();
        if (requesterId is null)
        {
            return Unauthorized();
        }
 
        var result = await _sender.Send(new GetTrendingFeedQuery(requesterId.Value, cursor, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 8.4 — following feed plus suggested users' posts mixed in.</summary>
    [HttpGet("personalized")]
    public async Task<IActionResult> Personalized([FromQuery] string? cursor, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var requesterId = await ResolveRequesterIdAsync();
        if (requesterId is null)
        {
            return Unauthorized();
        }
 
        var result = await _sender.Send(new GetPersonalizedFeedQuery(requesterId.Value, cursor, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }
 
    private Task<Guid?> ResolveRequesterIdAsync() =>
        Task.FromResult(Guid.TryParse(_currentUserService.UserId, out var id) ? id : (Guid?)null);
}