using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maydan.Application.DTOs.ServiceRequests;
using Maydan.Application.Interfaces;

namespace Maydan.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ServiceRequestsController : ApiControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet("resolve-association")]
    public async Task<ActionResult<AssociationLookupDto>> ResolveAssociation([FromQuery] int cityId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _serviceRequestService.ResolveAssociationByCityIdAsync(currentUserId, cityId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> Create([FromBody] CreateServiceRequestDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            var result = await _serviceRequestService.CreateAsync(currentUserId, dto, cancellationToken);
            return CreatedResponse($"/api/ServiceRequests/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("calculate-expected-payment")]
    public async Task<ActionResult<ExpectedPaymentCalculationDto>> Calculate([FromBody] CalculationRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized(new { message = "Current user id is required." });
            }

            var calculationResult = await _serviceRequestService.CalculateExpectedPaymentAsync(currentUserId, dto.ServiceId, dto.RequestedWorkers, dto.ShiftsOrDaysCount, cancellationToken);
            return Success(calculationResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceRequestDto>>> GetAll(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _serviceRequestService.GetAllAsync(currentUserId, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _serviceRequestService.GetByIdAsync(currentUserId, id, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            await _serviceRequestService.CancelAsync(currentUserId, id, cancellationToken);
            return NoContentResponse<ServiceRequestDto>();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
