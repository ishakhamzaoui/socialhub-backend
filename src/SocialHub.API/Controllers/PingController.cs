using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Diagnostics.Ping;
 
namespace SocialHub.API.Controllers;
 
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PingController : ControllerBase
{
    private readonly ISender _sender;
 
    public PingController(ISender sender)
    {
        _sender = sender;
    }
 
    /// <summary>Trivial success path through the full CQRS pipeline.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PingQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Trivial failure path — verifies Result -> ProblemDetails mapping.</summary>
    [HttpGet("fail")]
    public async Task<IActionResult> Fail(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FailPingQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
}