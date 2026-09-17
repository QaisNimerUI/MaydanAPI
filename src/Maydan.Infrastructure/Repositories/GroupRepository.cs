using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly MaydanDbContext _context;

    public GroupRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Group?> GetByIdAsync(int groupId, CancellationToken cancellationToken = default) =>
        _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

    public Task<Group?> GetDetailsAsync(int groupId, CancellationToken cancellationToken = default) =>
        _context.Groups
            .Include(g => g.GroupPermissions)
                .ThenInclude(gp => gp.Permission)
            .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                    .ThenInclude(u => u.Role)
            .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                    .ThenInclude(u => u.UserPermissions)
            .FirstOrDefaultAsync(g => g.GroupId == groupId, cancellationToken);

    public Task<List<Group>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Groups.ToListAsync(cancellationToken);

    public Task<List<Group>> GetByEntityAsync(EntityType entityType, int entityId, CancellationToken cancellationToken = default) =>
        _context.Groups
            .Include(g => g.GroupPermissions)
            .Include(g => g.UserGroups)
            .Where(g => g.EntityType == entityType && g.EntityId == entityId)
            .OrderBy(g => g.GroupNameEn)
            .ToListAsync(cancellationToken);

    public Task<List<Group>> GetByIdsInEntityAsync(IEnumerable<int> groupIds, EntityType entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var ids = groupIds.Distinct().ToList();

        return _context.Groups
            .Where(g => ids.Contains(g.GroupId) && g.EntityType == entityType && g.EntityId == entityId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> NameExistsInEntityAsync(EntityType entityType, int entityId, string groupNameEn, string groupNameAr, int? excludedGroupId = null, CancellationToken cancellationToken = default)
    {
        var normalizedNameEn = groupNameEn.Trim();
        var normalizedNameAr = groupNameAr.Trim();

        return _context.Groups.AnyAsync(g =>
            g.EntityType == entityType &&
            g.EntityId == entityId &&
            (!excludedGroupId.HasValue || g.GroupId != excludedGroupId.Value) &&
            (g.GroupNameEn == normalizedNameEn || g.GroupNameAr == normalizedNameAr),
            cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default) =>
        await _context.Groups.AddAsync(group, cancellationToken);

    public void Remove(Group group) => _context.Groups.Remove(group);
}
