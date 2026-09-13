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

    public async Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(
            currentUserId,
            cancellationToken);

        if (currentUser is null)
            throw new Exception("Current user not found.");

        if (currentUser.EntityType != EntityType.ProductionCompany)
            throw new Exception(
                "Only Production Company users can create projects.");

        if (string.IsNullOrWhiteSpace(request.ProjectNameEn))
            throw new Exception(
                "English project name is required.");

        if (string.IsNullOrWhiteSpace(request.ProjectNameAr))
            throw new Exception(
                "Arabic project name is required.");

        if (request.ProjectNameEn.Trim().Length > 200)
            throw new Exception(
                "English project name cannot exceed 200 characters.");

        if (request.ProjectNameAr.Trim().Length > 200)
            throw new Exception(
                "Arabic project name cannot exceed 200 characters.");

        if (request.EndDate <= request.StartDate)
            throw new Exception(
                "End date must be greater than start date.");

        if (request.ProducerUserId == request.LocationManagerUserId)
            throw new Exception(
                "Producer and Location Manager must be different users.");

        var projectTypeExists =
            await _unitOfWork.ProjectTypes.ExistsAsync(
                request.ProjectTypeId,
                cancellationToken);

        if (!projectTypeExists)
            throw new Exception("Invalid Project Type.");

        var producer =
            await _unitOfWork.Users.GetByIdAsync(
                request.ProducerUserId,
                cancellationToken);

        if (producer is null)
            throw new Exception("Producer not found.");

        var locationManager =
            await _unitOfWork.Users.GetByIdAsync(
                request.LocationManagerUserId,
                cancellationToken);

        if (locationManager is null)
            throw new Exception("Location Manager not found.");

        if (producer.EntityType != EntityType.ProductionCompany ||
            producer.EntityId != currentUser.EntityId)
        {
            throw new Exception(
                "Producer must belong to the same Production Company.");
        }

        if (locationManager.EntityType != EntityType.ProductionCompany ||
            locationManager.EntityId != currentUser.EntityId)
        {
            throw new Exception(
                "Location Manager must belong to the same Production Company.");
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

            WorkPermitImagePath = request.WorkPermitImagePath,

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
            throw new Exception(
                "Project could not be loaded after creation.");

        return MapToDto(createdProject);
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