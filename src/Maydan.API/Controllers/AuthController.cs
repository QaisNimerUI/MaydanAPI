using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

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
}
