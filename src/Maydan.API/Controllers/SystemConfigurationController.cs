using Maydan.API.Filters;
using Maydan.Application.DTOs.SystemConfiguration;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// System Configuration gate (MAYD-133, 2026-09-24). [Authorize] only enforces "any authenticated
// user" at the framework level, same as every other controller here (Program.cs's AddAuthorization()
// has no custom policies) — the real "is this Bayt-AlUrdon staff holding the Manage System
// Configuration permission" check lives in SystemConfigurationService, same place
// EntityOnboardingService's own equivalent check lives.
//
// [BypassSystemConfigurationGate] on the whole controller: this is the one place the gate's own
// target (an unconfigured super admin) must always be able to reach, or they could never actually
// finish configuring anything.
[Authorize]
[BypassSystemConfigurationGate]
[Route("api/system-configuration")]
public class SystemConfigurationController : ApiControllerBase
{
    private readonly ISystemConfigurationService _systemConfigurationService;

    public SystemConfigurationController(ISystemConfigurationService systemConfigurationService)
    {
        _systemConfigurationService = systemConfigurationService;
    }

    [HttpGet(Name = "Get System Configuration")]
    public async Task<ActionResult<SystemConfigurationDto>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _systemConfigurationService.GetAsync(currentUserId, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut(Name = "Update System Configuration")]
    public async Task<ActionResult<SystemConfigurationDto>> Update([FromBody] UpdateSystemConfigurationDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new { message = "Current user id is required." });
        }

        try
        {
            return Ok(await _systemConfigurationService.UpdateAsync(currentUserId, dto, cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
