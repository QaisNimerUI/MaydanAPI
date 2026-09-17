using Maydan.Application.DTOs.Auth;

namespace Maydan.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}
