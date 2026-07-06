using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialHub.API.Extensions;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
 
namespace SocialHub.API.Controllers;
 
/// <summary>
/// Roadmap 4.1-4.7: Media Infrastructure. Every action requires
/// authentication ([Authorize] here, plus IRequireAuthorization on every
/// command/query itself, per spec §2's "authorization checks occur in the
/// Application layer via pipeline behaviors, not solely in controllers").
///
/// Download/thumbnail are dedicated authorized endpoints rather than static
/// file middleware — chosen specifically because Phase 5/6 visibility rules
/// will need to gate access per-request once they exist, which static file
/// serving can't do (see GetMediaQueryHandler's remarks on the interim
/// owner-only rule).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorageService;
 
    public MediaController(ISender sender, IFileStorageService fileStorageService)
    {
        _sender = sender;
        _fileStorageService = fileStorageService;
    }
 
    /// <summary>Roadmap 4.1: Upload API.</summary>
    [HttpPost]
    [RequestSizeLimit(500 * 1024 * 1024)] // matches UploadMediaCommandValidator's video ceiling
    public async Task<IActionResult> Upload([FromForm] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "A file is required.");
        }
 
        await using var stream = request.File.OpenReadStream();
 
        var command = new UploadMediaCommand(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.Category);
 
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMediaQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteMediaCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
 
    /// <summary>Roadmap 4.1 exit criterion: "retrieved via a stable URL".</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMediaFileQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }
 
        var stream = await _fileStorageService.OpenReadAsync(result.Value.RelativePath, cancellationToken);
        return File(stream, result.Value.MimeType, result.Value.FileName);
    }
 
    [HttpGet("{id:guid}/thumbnail")]
    public async Task<IActionResult> Thumbnail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMediaFileQuery(id, Thumbnail: true), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }
 
        var stream = await _fileStorageService.OpenReadAsync(result.Value.RelativePath, cancellationToken);
        return File(stream, result.Value.MimeType, result.Value.FileName);
    }
}
 
/// <summary>[FromForm]-bound multipart upload payload: one file plus the destination category.</summary>
public sealed class UploadMediaRequest
{
    public IFormFile? File { get; set; }
 
    public MediaCategory Category { get; set; }
}