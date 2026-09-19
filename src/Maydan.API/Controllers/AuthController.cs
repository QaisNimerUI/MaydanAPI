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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}
