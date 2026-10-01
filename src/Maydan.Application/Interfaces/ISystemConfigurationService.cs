using Maydan.Application.DTOs.SystemConfiguration;

namespace Maydan.Application.Interfaces;

public interface ISystemConfigurationService
{
    Task<SystemConfigurationDto> GetAsync(int currentUserId, CancellationToken cancellationToken = default);

    Task<SystemConfigurationDto> UpdateAsync(int currentUserId, UpdateSystemConfigurationDto dto, CancellationToken cancellationToken = default);

    // No currentUserId here on purpose — called from SystemConfigurationGateService for every
    // authenticated super-admin request, not just from the config screen's own controller actions.
    Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default);
}
