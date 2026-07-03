using Microsoft.AspNetCore.Mvc;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.API.Extensions;
 
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller) =>
        result.IsSuccess ? controller.NoContent() : CreateProblemResult(result.Error, controller);
 
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller) =>
        result.IsSuccess ? controller.Ok(result.Value) : CreateProblemResult(result.Error, controller);
 
    private static IActionResult CreateProblemResult(Error error, ControllerBase controller)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
 
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = controller.HttpContext.Request.Path
        };
 
        if (error is ValidationError validationError)
        {
            problemDetails.Extensions["errors"] = validationError.Errors
                .Select(e => new { code = e.Code, message = e.Message });
        }
 
        problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;
 
        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}