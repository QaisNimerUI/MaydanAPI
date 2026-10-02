using Maydan.Application.DTOs.ProductionCompanies;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Production House Management (MAYD-80/81/82) — the ProductionCompaniesController
// AuthController.RegisterProductionCompany's own comment said didn't exist yet. Route/verb shape
// matches workforcment's production-company.service.ts exactly: GET (list, search + isDeleted
// query params together — one endpoint, not AssociationsController's four-separate-routes split),
// GET/{id} (details).
//
// Status route: PATCH {id}/status/update, matching UsersController.UpdateUserStatus's own shape
// (a one-field toggle, PATCH not PUT) rather than CityController.Restore's restore/{id} shape —
// Restore undoes a soft-delete (a different operation this module doesn't have at all, see this
// controller's own comment on GetAll's isDeleted param), while this is a plain active/inactive
// toggle, the exact same semantic operation UsersController.UpdateUserStatus already names this way.
//
// [Authorize]-only at the controller level (matches every other controller in this codebase) — the
// REAL enforcement is server-side in ProductionCompanyService.GetAuthorizedUserAsync (View-or-Manage
// for list/details, Manage-only for the status toggle), not a bare attribute.
[Authorize]
[Route("api/[controller]")]
public class ProductionCompanyController : ApiControllerBase
{
    private readonly IProductionCompanyService _productionCompanyService;

    public ProductionCompanyController(IProductionCompanyService productionCompanyService)
    {
        _productionCompanyService = productionCompanyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductionCompanyDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] bool isDeleted, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _productionCompanyService.GetAllAsync(currentUserId, search, isDeleted, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductionCompanyDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _productionCompanyService.GetByIdAsync(currentUserId, id, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPatch("{id:int}/status/update", Name = "Update Production Company Status")]
    public async Task<ActionResult<ProductionCompanyDto>> UpdateStatus(
        int id, [FromBody] UpdateProductionCompanyStatusDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _productionCompanyService.UpdateStatusAsync(currentUserId, id, dto, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
