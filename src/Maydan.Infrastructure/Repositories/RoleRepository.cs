using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly MaydanDbContext _context;

    public RoleRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default) =>
        _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

    public Task<Role?> GetWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default) =>
        _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Roles.ToListAsync(cancellationToken);

    public Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default) =>
        _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.RoleNameEn)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default) =>
        await _context.Roles.AddAsync(role, cancellationToken);

    public void Remove(Role role) => _context.Roles.Remove(role);
}
