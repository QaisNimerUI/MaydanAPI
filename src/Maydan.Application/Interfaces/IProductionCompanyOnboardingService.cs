using Maydan.Application.DTOs.Auth;

namespace Maydan.Application.Interfaces;

public interface IProductionCompanyOnboardingService
{
    Task<RegisterProductionCompanyResponseDto> RegisterAsync(RegisterProductionCompanyDto dto, CancellationToken cancellationToken = default);
}
