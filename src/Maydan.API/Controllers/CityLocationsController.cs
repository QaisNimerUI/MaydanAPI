using Maydan.Application.DTOs.Locations;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Association Management, Phase 2d (2026-09-26) — the last piece of the original "Phase 1" Locations
// scope (Country/City). Route/verb shape matches workforcment's CityLocationsService exactly
// (src/app/features/associations/services/city-locations.service.ts): GET city/{cityId} (route
// param, not a query param), POST, PUT (id in the body — same convention
// AssociationsController.Update/ProjectsController.Update already use), DELETE/{cityLocationId} and
// PATCH restore/{cityLocationId} (both route params — CityController's own convention, NOT
// AssociationsController's DELETE ?id= convention).
//
// Unlike CountryController/CityController, this controller is [Authorize]-only everywhere,
// including its read endpoint — confirmed by checking every real caller of
// CityLocationsService.getByCity(): only location-management.component.ts (the admin Locations
// page, itself gated behind roleGuard/pageAccessGuard requiring Manage/View Locations permission in
// workforcment's app.routes.ts). There is no public, no-session consumer analogous to
// AuthController.RegisterProductionCompany's Production House signup dropdown (the actual reason
// CountryController/CityController's read endpoints are [AllowAnonymous] — see CountryController's
// own header comment), so that precedent does not apply here and carrying it over would needlessly
// expose this list to unauthenticated callers.
[Authorize]
[Route("api/[controller]")]
public class CityLocationsController : ApiControllerBase
{
    private readonly ILocationService _locationService;

    public CityLocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("city/{cityId:int}")]
    public async Task<ActionResult<List<CityLocationDto>>> GetByCity(int cityId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _locationService.GetCityLocationsByCityAsync(cityId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    public async Task<ActionResult<CityLocationDto>> Create([FromBody] CreateCityLocationDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _locationService.CreateCityLocationAsync(dto, currentUserId, cancellationToken);
            return Created($"/api/CityLocations/{result.Id}", result);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut]
    public async Task<ActionResult<CityLocationDto>> Update([FromBody] UpdateCityLocationDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.UpdateCityLocationAsync(dto, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{cityLocationId:int}")]
    public async Task<IActionResult> Delete(int cityLocationId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _locationService.DeleteCityLocationAsync(cityLocationId, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPatch("restore/{cityLocationId:int}")]
    public async Task<ActionResult<CityLocationDto>> Restore(int cityLocationId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.RestoreCityLocationAsync(cityLocationId, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
