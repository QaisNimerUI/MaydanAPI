using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    // Projects CRUD: MaydanDbContext applies a global soft-delete query filter
    // (!IsDeleted) to every SharedEntities type, so GetByIdAsync above will never return a
    // soft-deleted project. RestoreAsync needs to find one anyway — this bypasses the filter.
    Task<Project?> GetByIdIncludingDeletedAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<List<Project>> GetByProductionCompanyIdAsync(
        int productionCompanyId,
        CancellationToken cancellationToken = default);

    // Backs GET /api/Projects. Mirrors the exact filter set workforcment's projects.service.ts
    // already sends (getAll()'s ProjectsQuery): isDeleted picks the soft-deleted view (bypassing
    // the global query filter) vs. the normal active view; the rest are optional narrowing filters.
    Task<List<Project>> GetAllAsync(
        bool isDeleted,
        string? searchTerm,
        DateTime? startDate,
        DateTime? endDate,
        string? searchByProductionCompanyName,
        int? searchByProductionCompanyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    void Remove(Project project);
}
