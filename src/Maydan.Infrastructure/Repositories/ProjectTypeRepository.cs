using Maydan.Application.Interfaces;
using Maydan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maydan.Infrastructure.Repositories;

public class ProjectTypeRepository : IProjectTypeRepository
{
    private readonly MaydanDbContext _context;

    public ProjectTypeRepository(MaydanDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(
        int projectTypeId,
        CancellationToken cancellationToken = default)
    {
        return _context.ProjectTypes.AnyAsync(
            x => x.Id == projectTypeId &&
                 x.IsActive,
            cancellationToken);
    }
}