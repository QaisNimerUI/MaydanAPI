using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

[Authorize]
[Route("api/groups")]
public class GroupsController : ApiControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public GroupsController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("current-entity", Name = "Get Current Entity Groups")]
    public async Task<ActionResult<List<GroupSummaryDto>>> GetCurrentEntityGroups([FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetGroupsAsync(currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{groupId:int}/details", Name = "Get Group Details")]
    public async Task<ActionResult<GroupDetailsDto>> GetGroup(int groupId, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.GetGroupAsync(currentUserId, groupId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost("create", Name = "Create Permission Group")]
    public async Task<ActionResult<GroupDetailsDto>> CreatePermissionGroup([FromBody] CreateGroupDto dto, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var group = await _userManagementService.CreateGroupAsync(currentUserId, dto, cancellationToken);
            return Created($"/api/groups/{group.GroupId}/details", group);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{groupId:int}/update", Name = "Update Permission Group")]
    public async Task<ActionResult<GroupDetailsDto>> UpdatePermissionGroup(int groupId, [FromBody] UpdateGroupDto dto, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _userManagementService.UpdateGroupAsync(currentUserId, groupId, dto, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{groupId:int}/delete", Name = "Delete Permission Group")]
    public async Task<IActionResult> DeletePermissionGroup(int groupId, [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _userManagementService.DeleteGroupAsync(currentUserId, groupId, cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
