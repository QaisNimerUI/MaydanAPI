using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
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

    public Task<List<Group>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Groups.ToListAsync(cancellationToken);

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default) =>
        await _context.Groups.AddAsync(group, cancellationToken);

    public void Remove(Group group) => _context.Groups.Remove(group);
}
