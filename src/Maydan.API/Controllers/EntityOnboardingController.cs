using Maydan.Application.DTOs.Onboarding;
using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Entity onboarding Stage 2 (2026-09-22): Bayt-AlUrdon picking an existing, admin-less Association
// and creating its first admin. [Authorize] only enforces "any authenticated user" at the
// framework level, same as every other controller here — there is no per-permission
// policy/attribute mechanism anywhere in this API (Program.cs's AddAuthorization() has no custom
// policies). The real "does this caller hold permission 37 and are they Bayt-AlUrdon" check lives
// in EntityOnboardingService, the same place UserManagementService.EnsureSameEntityCreation
// enforces its own guard.
[Authorize]
[Route("api/entity-onboarding")]
public class EntityOnboardingController : ApiControllerBase
{
    private readonly IEntityOnboardingService _entityOnboardingService;

    public EntityOnboardingController(IEntityOnboardingService entityOnboardingService)
    {
        _entityOnboardingService = entityOnboardingService;
    }

    [HttpGet("associations/without-admin", Name = "Get Associations Without Admin")]
    public async Task<ActionResult<List<AssociationWithoutAdminDto>>> GetAssociationsWithoutAdmin(
        [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _entityOnboardingService.GetAssociationsWithoutAdminAsync(currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost("associations/{associationId:int}/admin", Name = "Onboard Association Admin")]
    public async Task<ActionResult<UserDetailsDto>> OnboardAssociationAdmin(
        int associationId, [FromBody] OnboardAssociationAdminDto dto,
        [FromHeader(Name = "X-Current-User-Id")] int? currentUserHeader, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var user = await _entityOnboardingService.OnboardAssociationAdminAsync(currentUserId, associationId, dto, cancellationToken);
            return Created($"/api/users/{user.UserId}/details", user);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
