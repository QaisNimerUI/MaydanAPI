using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("current-entity", Name = "Get Current Entity Users")]
    public async Task<ActionResult<List<UserSummaryDto>>> GetCurrentEntityUsers([FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetUsersAsync(currentUserId, null, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("search", Name = "Search Current Entity Users")]
    public async Task<ActionResult<List<UserSummaryDto>>> SearchCurrentEntityUsers([FromQuery] string? search, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetUsersAsync(currentUserId, search, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{userId:int}/details", Name = "Get User Details")]
    public async Task<ActionResult<UserDetailsDto>> GetUserDetails(int userId, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetUserDetailsAsync(currentUserId, userId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost("register", Name = "Register User")]
    public async Task<ActionResult<UserDetailsDto>> RegisterUser([FromBody] CreateEntityUserDto dto, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var user = await _userManagementService.CreateUserAsync(currentUserId, dto, cancellationToken);
            return Created($"/api/users/{user.UserId}/details", user);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{userId:int}/direct-permissions/update", Name = "Update User Direct Permissions")]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserDirectPermissions(int userId, [FromBody] UpdateUserPermissionsDto dto, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.UpdateDirectPermissionsAsync(currentUserId, userId, dto, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{userId:int}/groups/update", Name = "Update User Groups")]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserGroups(int userId, [FromBody] UpdateUserGroupsDto dto, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.UpdateGroupsAsync(currentUserId, userId, dto, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{userId:int}/effective-permissions", Name = "Get User Effective Permissions")]
    public async Task<ActionResult<List<PermissionDto>>> GetUserEffectivePermissions(int userId, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetEffectivePermissionsAsync(currentUserId, userId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
