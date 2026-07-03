using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Features.Hashtags;
 
namespace SocialHub.API.Controllers;
 
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class HashtagsController : ControllerBase
{
    private readonly ISender _sender;
 
    public HashtagsController(ISender sender)
    {
        _sender = sender;
    }
 
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetHashtagsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHashtagCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}