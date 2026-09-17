using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithAccessAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User>> GetByEntityAsync(EntityType entityType, int entityId, string? search, CancellationToken cancellationToken = default);
    Task<User?> GetDetailsAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetDetailsReadOnlyAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default);

    // Loads Role, Groups and direct UserPermissions so the effective-permissions union (section 4) can be computed.
    Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<User>> GetByIdsInEntityAsync(IEnumerable<int> userIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Remove(User user);
}
