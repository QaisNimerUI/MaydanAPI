using Maydan.Application.DTOs.Projects;

namespace Maydan.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default);
}