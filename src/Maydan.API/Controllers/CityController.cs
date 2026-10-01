using Maydan.Application.DTOs.Locations;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Same phase/rationale as CountryController — see that controller's own header comment for why the
// read endpoint is [AllowAnonymous] (the public Production House signup dropdown) while writes stay
// [Authorize]-only. Route/verb shape matches workforcment's CitiesService exactly
// (src/app/features/locations/services/cities.service.ts): the one list endpoint is scoped by
// country (country-cities?CountryId=), not a flat GET-all — there is no unscoped "all cities"
// consumer anywhere in the frontend today.
[Route("api/[controller]")]
public class CityController : ApiControllerBase
{
    private readonly ILocationService _locationService;

    public CityController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [AllowAnonymous]
    [HttpGet("country-cities")]
    public async Task<ActionResult<List<CityDto>>> GetByCountry([FromQuery] int countryId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _locationService.GetCitiesByCountryAsync(countryId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CityDto>> Create([FromBody] CreateCityDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _locationService.CreateCityAsync(dto, currentUserId, cancellationToken);
            return Created($"/api/City/{result.Id}", result);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPut("{cityId:int}")]
    public async Task<ActionResult<CityDto>> Update(int cityId, [FromBody] UpdateCityDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.UpdateCityAsync(cityId, dto, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpDelete("{cityId:int}")]
    public async Task<IActionResult> Delete(int cityId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _locationService.DeleteCityAsync(cityId, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPatch("restore/{cityId:int}")]
    public async Task<ActionResult<CityDto>> Restore(int cityId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _locationService.RestoreCityAsync(cityId, currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
