using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface ISystemConfigurationRepository
{
    // Singleton table — at most one row. Null means "never configured yet".
    Task<SystemConfiguration?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default);
}
