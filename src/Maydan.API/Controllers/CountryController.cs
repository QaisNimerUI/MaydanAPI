using Maydan.Application.DTOs.Locations;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Associations/Country/City backend gap, Phase 1 (2026-09-25) — see LocationService's own header
// comment for the full "what was already built vs. what was missing" story.
//
// Route/verb shape matches workforcment's CountriesService exactly (src/app/features/locations/
// services/countries.service.ts), not a convention invented here: GET (list) + GET/{id}, POST, PUT
// (id in the body — same unusual-but-real convention ProjectsController.Update already uses, see
// its own comment), DELETE/{countryId}, PATCH restore/{countryId}.
//
// Read endpoints are [AllowAnonymous] deliberately, not just [Authorize] like the rest of this
// controller: AuthController.RegisterProductionCompany (the public, no-session Production House
// signup flow) is the confirmed real caller of GET /api/Country and GET /api/City/country-cities
// via ProductionHouseSignupComponent's own country/city dropdowns — the same real bug this whole
// phase exists to fix. Gating the list endpoints behind [Authorize] would 401 that flow before a
// user has ever logged in. Writes stay [Authorize]-only (no dedicated permission bit), matching the
// same house-style precedent GroupsController/ProjectsController already established for
// comparable CRUD — neither checks a specific permission at the controller or service layer beyond
// "is this a valid, active, authenticated user."
[Route("api/[controller]")]
public class CountryController : ApiControllerBase
{
    private readonly ILocationService _locationService;

    public CountryController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CountryDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _locationService.GetCountriesAsync(cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CountryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _locationService.GetCountryByIdAsync(id, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CountryDto>> Create([FromBody] CreateCountryDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _locationService.CreateCountryAsync(dto, currentUserId, cancellationToken);
            return Created($"/api/Country/{result.Id}", result);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<CountryDto>> Update([FromBody] UpdateCountryDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.UpdateCountryAsync(dto, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpDelete("{countryId:int}")]
    public async Task<IActionResult> Delete(int countryId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _locationService.DeleteCountryAsync(countryId, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPatch("restore/{countryId:int}")]
    public async Task<ActionResult<CountryDto>> Restore(int countryId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.RestoreCountryAsync(countryId, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
