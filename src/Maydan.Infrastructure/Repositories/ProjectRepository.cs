using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly MaydanDbContext _context;

    public ProjectRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<Project?> GetByIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        return _context.Projects
            .Include(x => x.ProjectType)
            .Include(x => x.Producer)
            .Include(x => x.LocationManager)
            .Include(x => x.ProductionCompany)
            .FirstOrDefaultAsync(
                x => x.Id == projectId,
                cancellationToken);
    }

    public Task<List<Project>> GetByProductionCompanyIdAsync(
        int productionCompanyId,
        CancellationToken cancellationToken = default)
    {
        return _context.Projects
            .AsNoTracking()
            .Include(x => x.ProjectType)
            .Include(x => x.Producer)
            .Include(x => x.LocationManager)
            .Where(x => x.ProductionCompanyId == productionCompanyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    public void Remove(Project project)
    {
        _context.Projects.Remove(project);
    }
}