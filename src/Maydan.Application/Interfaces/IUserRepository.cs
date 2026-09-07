using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default);

    // Loads Role, Groups and direct UserPermissions so the effective-permissions union (section 4) can be computed.
    Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Remove(User user);
}
