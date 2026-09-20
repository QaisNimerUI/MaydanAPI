using Maydan.Application.DTOs.Projects;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Services;

// Projects audit follow-up: every validation failure in this class used to throw a bare
// Exception, which ApiControllerBase.HandleException's switch has no case for — it always fell
// through to the generic 500 branch regardless of whether the real problem was a 400 (bad
// input/business rule) or a 404 (referenced entity not found). Every throw below now uses the
// same exception-type convention UserManagementService already established:
// InvalidOperationException for business-rule violations, KeyNotFoundException for "not found",
// UnauthorizedAccessException for acting outside the caller's own entity boundary (mirrors
// UserManagementService.GetScopedUserAsync()'s "Cannot manage users outside the current entity").
//
// NOTE (deliberately out of scope): linking a Project to Locations/Associations is blocked on the
// GIS team providing real location data — no field, table, or DTO for that exists here, and none
// should be added until that data is available.
public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);

        if (currentUser.EntityType != EntityType.ProductionCompany)
        {
            throw new InvalidOperationException("Only Production Company users can create projects.");
        }

        ValidateProjectPayload(
            request.ProjectNameEn,
            request.ProjectNameAr,
            request.StartDate,
            request.EndDate,
            request.ProducerUserId,
            request.LocationManagerUserId);

        if (!await _unitOfWork.ProjectTypes.ExistsAsync(request.ProjectTypeId, cancellationToken))
        {
            throw new KeyNotFoundException("Invalid Project Type.");
        }

        var producer = await GetSameCompanyUserAsync(request.ProducerUserId, currentUser, "Producer", cancellationToken);
        var locationManager = await GetSameCompanyUserAsync(request.LocationManagerUserId, currentUser, "Location Manager", cancellationToken);

        var project = new Project
        {
            ProjectNameEn = request.ProjectNameEn.Trim(),
            ProjectNameAr = request.ProjectNameAr.Trim(),

            StartDate = request.StartDate,
            EndDate = request.EndDate,

            ProjectTypeId = request.ProjectTypeId,

            ProducerUserId = producer.UserId,
            LocationManagerUserId = locationManager.UserId,

            WorkPermitImagePath = request.WorkPermitImagePath,

            ProductionCompanyId = currentUser.EntityId,

            CreatedBy = currentUser.UserId,
            IsActive = true
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdProject = await _unitOfWork.Projects.GetByIdAsync(project.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Project could not be loaded after creation.");

        return MapToDto(createdProject);
    }

    public async Task<List<ProjectDto>> GetAllAsync(
        ProjectQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync(
            query.IsDeleted,
            query.SearchTerm,
            query.StartDate,
            query.EndDate,
            query.SearchByProductionCompanyName,
            query.SearchByProductionCompanyId,
            cancellationToken);

        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(
        int id,
        UpdateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        EnsureSameProductionCompany(project, currentUser);

        ValidateProjectPayload(
            request.ProjectNameEn,
            request.ProjectNameAr,
            request.StartDate,
            request.EndDate,
            request.ProducerUserId,
            request.LocationManagerUserId);

        if (!await _unitOfWork.ProjectTypes.ExistsAsync(request.ProjectTypeId, cancellationToken))
        {
            throw new KeyNotFoundException("Invalid Project Type.");
        }

        var producer = await GetSameCompanyUserAsync(request.ProducerUserId, currentUser, "Producer", cancellationToken);
        var locationManager = await GetSameCompanyUserAsync(request.LocationManagerUserId, currentUser, "Location Manager", cancellationToken);

        project.ProjectNameEn = request.ProjectNameEn.Trim();
        project.ProjectNameAr = request.ProjectNameAr.Trim();
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.ProjectTypeId = request.ProjectTypeId;
        project.ProducerUserId = producer.UserId;
        project.LocationManagerUserId = locationManager.UserId;

        // Null/empty = no new file uploaded on this edit — keep the existing stored path.
        if (!string.IsNullOrWhiteSpace(request.WorkPermitImagePath))
        {
            project.WorkPermitImagePath = request.WorkPermitImagePath;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedProject = await _unitOfWork.Projects.GetByIdAsync(project.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Project could not be loaded after update.");

        return MapToDto(updatedProject);
    }

    public async Task DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        EnsureSameProductionCompany(project, currentUser);

        // MaydanDbContext.SaveChangesAsync intercepts EntityState.Deleted for every SharedEntities
        // and converts it into a soft delete (IsDeleted = true, DeletedAt = UtcNow) — this does not
        // hard-delete the row.
        _unitOfWork.Projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjectDto> RestoreAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);

        // GetByIdAsync would never find this project — the global soft-delete query filter
        // excludes it precisely because it's deleted. GetByIdIncludingDeletedAsync bypasses that.
        var project = await _unitOfWork.Projects.GetByIdIncludingDeletedAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        EnsureSameProductionCompany(project, currentUser);

        if (!project.IsDeleted)
        {
            throw new InvalidOperationException("Project is not deleted.");
        }

        project.IsDeleted = false;
        project.DeletedAt = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(project);
    }

    public async Task<List<ProjectTypeDto>> GetProjectTypesAsync(
        CancellationToken cancellationToken = default)
    {
        var projectTypes = await _unitOfWork.ProjectTypes.GetAllAsync(cancellationToken);

        return projectTypes
            .Select(projectType => new ProjectTypeDto(projectType.Id, projectType.NameEn, projectType.NameAr))
            .ToList();
    }

    private async Task<User> GetCurrentUserAsync(int currentUserId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user was not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Current user is inactive.");
        }

        return user;
    }

    // Shared by CreateAsync/UpdateAsync — the same field-level validation both need.
    private static void ValidateProjectPayload(
        string projectNameEn,
        string projectNameAr,
        DateTime startDate,
        DateTime endDate,
        int producerUserId,
        int locationManagerUserId)
    {
        if (string.IsNullOrWhiteSpace(projectNameEn))
        {
            throw new InvalidOperationException("English project name is required.");
        }

        if (string.IsNullOrWhiteSpace(projectNameAr))
        {
            throw new InvalidOperationException("Arabic project name is required.");
        }

        if (projectNameEn.Trim().Length > 200)
        {
            throw new InvalidOperationException("English project name cannot exceed 200 characters.");
        }

        if (projectNameAr.Trim().Length > 200)
        {
            throw new InvalidOperationException("Arabic project name cannot exceed 200 characters.");
        }

        if (endDate <= startDate)
        {
            throw new InvalidOperationException("End date must be greater than start date.");
        }

        if (producerUserId == locationManagerUserId)
        {
            throw new InvalidOperationException("Producer and Location Manager must be different users.");
        }
    }

    // Shared by CreateAsync/UpdateAsync — resolves a user and confirms it belongs to the same
    // Production Company as the current user, the exact rule CreateAsync already enforced for
    // both Producer and Location Manager.
    private async Task<User> GetSameCompanyUserAsync(
        int userId,
        User currentUser,
        string roleLabel,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"{roleLabel} not found.");

        if (user.EntityType != EntityType.ProductionCompany || user.EntityId != currentUser.EntityId)
        {
            throw new InvalidOperationException($"{roleLabel} must belong to the same Production Company.");
        }

        return user;
    }

    // Update/Delete/Restore all act on an EXISTING project — this confirms the caller's own
    // Production Company owns it. Same "acting outside your own entity" boundary
    // UserManagementService.GetScopedUserAsync() enforces for Users ("Cannot manage users outside
    // the current entity") — same exception type, same reasoning.
    private static void EnsureSameProductionCompany(Project project, User currentUser)
    {
        if (project.ProductionCompanyId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Cannot manage a project outside the current Production Company.");
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

            ProductionCompanyId = project.ProductionCompanyId,

            IsDeleted = project.IsDeleted
        };
    }
}
