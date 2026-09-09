using Maydan.Domain.Entities;

namespace Maydan.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<List<Project>> GetByProductionCompanyIdAsync(
        int productionCompanyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    void Remove(Project project);
}