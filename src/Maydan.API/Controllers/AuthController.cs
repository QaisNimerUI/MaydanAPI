using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Deliberately public: login and reset-password both authenticate the caller via credentials
// in the request body (email/password), not a Bearer token — there is no session yet at the
// point these actions run. AllowAnonymous is explicit here so this reads as an intentional
// choice, not an oversight, now that GroupsController/PermissionsController/UsersController
// all carry [Authorize].
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IProductionCompanyOnboardingService _productionCompanyOnboardingService;

    public AuthController(IAuthService authService, IProductionCompanyOnboardingService productionCompanyOnboardingService)
    {
        _authService = authService;
        _productionCompanyOnboardingService = productionCompanyOnboardingService;
    }

    [HttpPost("login", Name = "Login User")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _authService.LoginAsync(dto, cancellationToken));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost("reset-password", Name = "Reset Password")]
    public async Task<ActionResult<LoginResponseDto>> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _authService.ResetPasswordAsync(dto, cancellationToken));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    // Entity onboarding Stage 1 (2026-09-22): public production-company self-registration — same
    // AllowAnonymous rationale as the rest of this controller, since there's no session at all yet
    // (not even a user to log in as until this call succeeds). Instant activation (confirmed
    // product decision) is treated as "tell them it worked, they log in" rather than "land them
    // logged in": a fresh registration exercising the real LoginAsync path on the very next request
    // catches any auth-invariant bug immediately, rather than this endpoint quietly reimplementing
    // token issuance a second time for a one-time moment.
    [HttpPost("register-production-company", Name = "Register Production Company")]
    public async Task<ActionResult<RegisterProductionCompanyResponseDto>> RegisterProductionCompany(
        [FromBody] RegisterProductionCompanyDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productionCompanyOnboardingService.RegisterAsync(dto, cancellationToken);
            // No GET-by-id endpoint exists for production companies yet (no ProductionCompaniesController
            // at all) — this Location URI is a placeholder for when one is added, not a live route.
            return Created($"api/production-companies/{result.ProductionCompanyId}", result);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
