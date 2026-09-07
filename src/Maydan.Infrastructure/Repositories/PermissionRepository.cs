using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly MaydanDbContext _context;

    public PermissionRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Permission?> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default) =>
        _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == permissionId, cancellationToken);

    public Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Permissions.ToListAsync(cancellationToken);

    public Task<List<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default) =>
        _context.Permissions.Where(p => p.Module == module).ToListAsync(cancellationToken);

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default) =>
        await _context.Permissions.AddAsync(permission, cancellationToken);

    public void Remove(Permission permission) => _context.Permissions.Remove(permission);
}
