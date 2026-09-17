using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCurrentUserId(out int currentUserId)
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claimValue, out currentUserId))
        {
            return true;
        }

        if (Request.Headers.TryGetValue("X-Current-User-Id", out var values) &&
            int.TryParse(values.FirstOrDefault(), out currentUserId))
        {
            return true;
        }

        currentUserId = 0;
        return false;
    }

    protected ActionResult HandleException(Exception exception) =>
        exception switch
        {
            UnauthorizedAccessException => Forbid(),
            KeyNotFoundException => NotFound(new { message = exception.Message }),
            InvalidOperationException => BadRequest(new { message = exception.Message }),
            ArgumentException => BadRequest(new { message = exception.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected server error." })
        };
}
