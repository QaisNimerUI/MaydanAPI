using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    void Remove(Permission permission);
}
