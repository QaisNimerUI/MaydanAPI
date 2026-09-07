using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MaydanDbContext _context;

    public UserRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

    public Task<User?> GetByUserNameArAsync(User user, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.FirstNameAr==user.FirstNameAr && u.LastNameAr == user.LastNameAr, cancellationToken);

    public Task<User?> GetByUserNameEnAsync(User user, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.FirstNameEn == user.FirstNameEn && u.LastNameEn == user.LastNameEn, cancellationToken);

    public Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default) =>
        _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                    .ThenInclude(g => g.GroupPermissions)
                        .ThenInclude(gp => gp.Permission)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public void Remove(User user) => _context.Users.Remove(user);
}
