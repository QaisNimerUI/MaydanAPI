using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

[Route("api/permissions")]
public class PermissionsController : ApiControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public PermissionsController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("available", Name = "Get Available Permissions")]
    public async Task<ActionResult<List<PermissionDto>>> GetAvailablePermissions([FromQuery] int? roleId, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetAvailablePermissionsAsync(currentUserId, roleId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("matrix", Name = "Get Permission Matrix")]
    public async Task<ActionResult<PermissionMatrixDto>> GetPermissionMatrix([FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetPermissionMatrixAsync(currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
