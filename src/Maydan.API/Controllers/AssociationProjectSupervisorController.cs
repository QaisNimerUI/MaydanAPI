using Maydan.Application.DTOs.Associations;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Association Management, Phase 2e — route/verb shape matches workforcment's
// association-supervisors.service.ts exactly (src/app/features/associations/services/
// association-supervisors.service.ts): GET (list, projectId/associationId always sent, 0 meaning
// "no filter"), POST, DELETE/{id} (the assignment row's own id — a route param, matching
// CityController's convention, not AssociationsController's DELETE ?id=). No restore endpoint —
// the frontend service has none.
//
// Deliberately named "Supervisor" (singular) despite representing a collection of assignments —
// [Route("api/[controller]")] resolves the class name directly, and the real frontend calls
// /api/AssociationProjectSupervisor (singular). Pluralizing this class out of habit would 404 the
// real page.
//
// [Authorize]-only, no fine-grained permission-id check — confirmed ProjectsController (the
// closest real neighbor for this Projects-area action) has none either: no PermissionId constants,
// no GetEffectivePermissionIds/Overlaps check anywhere in ProjectService, just controller-level
// [Authorize] plus the frontend's own route guards. Matched that exactly rather than inventing a
// stricter scheme AssociationsController's own DeleteAssociationsPermissionId-style checks don't
// actually establish as a project-supervisor requirement.
[Authorize]
[Route("api/[controller]")]
public class AssociationProjectSupervisorController : ApiControllerBase
{
    private readonly IAssociationProjectSupervisorService _supervisorService;

    public AssociationProjectSupervisorController(IAssociationProjectSupervisorService supervisorService)
    {
        _supervisorService = supervisorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssociationProjectSupervisorDto>>> GetAll(
        [FromQuery] int projectId, [FromQuery] int associationId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _supervisorService.GetAllAsync(projectId, associationId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    public async Task<ActionResult<AssociationProjectSupervisorDto>> Create(
        [FromBody] CreateAssociationProjectSupervisorDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _supervisorService.CreateAsync(dto, currentUserId, cancellationToken);
            return Created($"/api/AssociationProjectSupervisor/{result.Id}", result);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _supervisorService.DeleteAsync(id, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
