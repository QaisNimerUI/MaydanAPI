using Maydan.Application.DTOs.Projects;

namespace Maydan.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> GetByIdAsync(
        int projectId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
