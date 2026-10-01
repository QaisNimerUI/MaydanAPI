using Maydan.Application.DTOs.Services;

namespace Maydan.Application.Interfaces;

public interface IServiceConfigurationService
{
    Task<List<ServiceDto>> GetAllServicesAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<List<ServiceDto>> GetActiveServicesAsync(CancellationToken cancellationToken = default);
    Task<ServiceDto> GetServiceByIdAsync(int currentUserId, int id, CancellationToken cancellationToken = default);
    Task<ServiceDto> CreateServiceAsync(int currentUserId, CreateServiceDto dto, CancellationToken cancellationToken = default);
    Task<ServiceDto> UpdateServiceAsync(int currentUserId, int id, UpdateServiceDto dto, CancellationToken cancellationToken = default);
    Task DeleteServiceAsync(int currentUserId, int id, CancellationToken cancellationToken = default);
}
