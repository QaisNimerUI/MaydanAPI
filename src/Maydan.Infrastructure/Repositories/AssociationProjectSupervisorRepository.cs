using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class AssociationProjectSupervisorRepository : IAssociationProjectSupervisorRepository
{
    private readonly MaydanDbContext _context;

    public AssociationProjectSupervisorRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<List<AssociationProjectSupervisor>> GetAllAsync(int? projectId, int? associationId, CancellationToken cancellationToken = default)
    {
        var query = _context.AssociationProjectSupervisors
            .Include(s => s.Project)
            .Include(s => s.User)
            .AsQueryable();

        if (projectId is not null)
        {
            query = query.Where(s => s.ProjectId == projectId);
        }

        if (associationId is not null)
        {
            query = query.Where(s => s.User.EntityType == EntityType.Association && s.User.EntityId == associationId);
        }

        return query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<AssociationProjectSupervisor?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.AssociationProjectSupervisors
            .Include(s => s.Project)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(AssociationProjectSupervisor supervisor, CancellationToken cancellationToken = default) =>
        await _context.AssociationProjectSupervisors.AddAsync(supervisor, cancellationToken);

    public void Remove(AssociationProjectSupervisor supervisor) => _context.AssociationProjectSupervisors.Remove(supervisor);
}
