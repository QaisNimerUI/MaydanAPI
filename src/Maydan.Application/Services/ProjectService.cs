using Maydan.Application.DTOs.Projects;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProjectDto>> GetAllAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentProductionCompanyUserAsync(currentUserId, cancellationToken);
        var projects = await _unitOfWork.Projects.GetByProductionCompanyIdAsync(
            currentUser.EntityId,
            cancellationToken);

        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(
        int projectId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentProductionCompanyUserAsync(currentUserId, cancellationToken);
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        EnsureProjectVisibleToUser(project, currentUser);

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentProductionCompanyUserAsync(currentUserId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.ProjectNameEn))
        {
            throw new InvalidOperationException("English project name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProjectNameAr))
        {
            throw new InvalidOperationException("Arabic project name is required.");
        }

        if (request.ProjectNameEn.Trim().Length > 200)
        {
            throw new InvalidOperationException("English project name cannot exceed 200 characters.");
        }

        if (request.ProjectNameAr.Trim().Length > 200)
        {
            throw new InvalidOperationException("Arabic project name cannot exceed 200 characters.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidOperationException("End date must be greater than start date.");
        }

        if (request.ProducerUserId == request.LocationManagerUserId)
        {
            throw new InvalidOperationException("Producer and Location Manager must be different users.");
        }

        if (string.IsNullOrWhiteSpace(request.WorkPermitImagePath))
        {
            throw new InvalidOperationException("Work permit image path is required.");
        }

        if (request.WorkPermitImagePath.Trim().Length > 500)
        {
            throw new InvalidOperationException("Work permit image path cannot exceed 500 characters.");
        }

        var projectTypeExists =
            await _unitOfWork.ProjectTypes.ExistsAsync(
                request.ProjectTypeId,
                cancellationToken);

        if (!projectTypeExists)
        {
            throw new InvalidOperationException("Invalid Project Type.");
        }

        var producer =
            await _unitOfWork.Users.GetByIdAsync(
                request.ProducerUserId,
                cancellationToken);

        if (producer is null)
        {
            throw new KeyNotFoundException("Producer was not found.");
        }

        var locationManager =
            await _unitOfWork.Users.GetByIdAsync(
                request.LocationManagerUserId,
                cancellationToken);

        if (locationManager is null)
        {
            throw new KeyNotFoundException("Location Manager was not found.");
        }

        if (producer.EntityType != EntityType.ProductionCompany ||
            producer.EntityId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Producer must belong to the same Production Company.");
        }

        if (locationManager.EntityType != EntityType.ProductionCompany ||
            locationManager.EntityId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Location Manager must belong to the same Production Company.");
        }

        var project = new Project
        {
            ProjectNameEn = request.ProjectNameEn.Trim(),
            ProjectNameAr = request.ProjectNameAr.Trim(),

            StartDate = request.StartDate,
            EndDate = request.EndDate,

            ProjectTypeId = request.ProjectTypeId,

            ProducerUserId = request.ProducerUserId,
            LocationManagerUserId = request.LocationManagerUserId,

            WorkPermitImagePath = request.WorkPermitImagePath.Trim(),

            ProductionCompanyId = currentUser.EntityId,

            CreatedBy = currentUser.UserId,
            IsActive = true
        };

        await _unitOfWork.Projects.AddAsync(
            project,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdProject =
            await _unitOfWork.Projects.GetByIdAsync(
                project.Id,
                cancellationToken);

        if (createdProject is null)
        {
            throw new KeyNotFoundException("Project could not be loaded after creation.");
        }

        return MapToDto(createdProject);
    }

    private async Task<User> GetCurrentProductionCompanyUserAsync(int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user was not found.");

        if (!currentUser.IsActive)
        {
            throw new UnauthorizedAccessException("Current user is inactive.");
        }

        if (currentUser.EntityType != EntityType.ProductionCompany)
        {
            throw new UnauthorizedAccessException("Only Production Company users can access projects.");
        }

        return currentUser;
    }

    private static void EnsureProjectVisibleToUser(Project project, User currentUser)
    {
        if (project.ProductionCompanyId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Cannot access projects outside the current Production Company.");
        }
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,

            ProjectNameEn = project.ProjectNameEn,
            ProjectNameAr = project.ProjectNameAr,

            StartDate = project.StartDate,
            EndDate = project.EndDate,

            ProjectTypeId = project.ProjectTypeId,
            ProjectTypeNameEn = project.ProjectType.NameEn,
            ProjectTypeNameAr = project.ProjectType.NameAr,

            ProducerUserId = project.ProducerUserId,
            ProducerNameEn =
                $"{project.Producer.FirstNameEn} {project.Producer.LastNameEn}",
            ProducerNameAr =
                $"{project.Producer.FirstNameAr} {project.Producer.LastNameAr}",

            LocationManagerUserId = project.LocationManagerUserId,
            LocationManagerNameEn =
                $"{project.LocationManager.FirstNameEn} {project.LocationManager.LastNameEn}",
            LocationManagerNameAr =
                $"{project.LocationManager.FirstNameAr} {project.LocationManager.LastNameAr}",

            WorkPermitImagePath = project.WorkPermitImagePath,

            ProductionCompanyId = project.ProductionCompanyId
        };
    }
}
