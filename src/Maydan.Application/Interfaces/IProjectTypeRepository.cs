using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IProjectTypeRepository
{
    Task<bool> ExistsAsync(
        int projectTypeId,
        CancellationToken cancellationToken = default);

    // Backs GET /api/ProjectTypes — the real seeded catalog (PermissionSeedConfiguration's sibling,
    // ProjectTypeSeed.cs, 15 rows) that project-form.component.ts's hardcoded
    // ['Productions', 'Tourism', 'Events'] list should have been reading from all along.
    Task<List<ProjectType>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
