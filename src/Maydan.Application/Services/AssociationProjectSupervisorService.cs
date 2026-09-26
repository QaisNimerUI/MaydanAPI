using Maydan.Application.DTOs.Associations;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;
using Maydan.Domain.Enums;

namespace Maydan.Application.Services;

public class AssociationProjectSupervisorService : IAssociationProjectSupervisorService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssociationProjectSupervisorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AssociationProjectSupervisorDto>> GetAllAsync(int projectId, int associationId, CancellationToken cancellationToken = default)
    {
        var projectFilter = projectId > 0 ? projectId : (int?)null;
        var associationFilter = associationId > 0 ? associationId : (int?)null;

        var supervisors = await _unitOfWork.AssociationProjectSupervisors.GetAllAsync(projectFilter, associationFilter, cancellationToken);
        var associationNames = await ResolveAssociationNamesAsync(supervisors, cancellationToken);

        return supervisors.Select(s => MapToDto(s, associationNames)).ToList();
    }

    public async Task<AssociationProjectSupervisorDto> CreateAsync(CreateAssociationProjectSupervisorDto dto, int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetCurrentUserAsync(currentUserId, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project was not found.");

        var supervisorUser = await _unitOfWork.Users.GetByIdAsync(dto.AssociationUserId, cancellationToken)
            ?? throw new KeyNotFoundException("User was not found.");

        // The feature's own name and home (under the Associations area) make this intent clear —
        // a "supervisor" here is specifically an Association-role user, the same real rows Phase
        // 2b's EntityId wiring scoped. Rejecting any other role up front avoids silently creating
        // an assignment the enriched list can never resolve an AssociationName for anyway (an
        // AssociationId only exists for EntityType.Association users).
        if (supervisorUser.EntityType != EntityType.Association)
        {
            throw new InvalidOperationException("Supervisor must be an Association-role user.");
        }

        var supervisor = new AssociationProjectSupervisor
        {
            ProjectId = project.Id,
            Project = project,
            UserId = supervisorUser.UserId,
            User = supervisorUser,
            CreatedBy = currentUserId,
            IsActive = true
        };

        await _unitOfWork.AssociationProjectSupervisors.AddAsync(supervisor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.AssociationProjectSupervisors.GetByIdAsync(supervisor.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Supervisor assignment could not be loaded after creation.");

        var associationNames = await ResolveAssociationNamesAsync(new[] { created }, cancellationToken);

        return MapToDto(created, associationNames);
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetCurrentUserAsync(currentUserId, cancellationToken);

        var supervisor = await _unitOfWork.AssociationProjectSupervisors.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Supervisor assignment was not found.");

        // MaydanDbContext.SaveChangesAsync intercepts EntityState.Deleted for every SharedEntities
        // and converts it into a soft delete (IsDeleted = true, DeletedAt = UtcNow) — this does not
        // hard-delete the row (same pattern as ProjectService.DeleteAsync).
        _unitOfWork.AssociationProjectSupervisors.Remove(supervisor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

    // User.EntityId is a soft FK (no single target table, see User.cs's own comment) — it can't be
    // Include()'d, so resolving each distinct association's name is a small separate lookup per
    // unique EntityId in the result set (not per row), via the same active-only
    // IAssociationRepository.GetByIdAsync every other CreateXAsync in this codebase already uses to
    // validate a real association.
    private async Task<Dictionary<int, string>> ResolveAssociationNamesAsync(IEnumerable<AssociationProjectSupervisor> supervisors, CancellationToken cancellationToken)
    {
        var associationIds = supervisors
            .Where(s => s.User.EntityType == EntityType.Association)
            .Select(s => s.User.EntityId)
            .Distinct();

        var associationNames = new Dictionary<int, string>();

        foreach (var associationId in associationIds)
        {
            var association = await _unitOfWork.Associations.GetByIdAsync(associationId, cancellationToken);
            if (association is not null)
            {
                associationNames[associationId] = association.EnglishName;
            }
        }

        return associationNames;
    }

    private static AssociationProjectSupervisorDto MapToDto(AssociationProjectSupervisor supervisor, IReadOnlyDictionary<int, string> associationNames)
    {
        var isAssociationUser = supervisor.User.EntityType == EntityType.Association;
        var associationId = isAssociationUser ? supervisor.User.EntityId : (int?)null;

        return new AssociationProjectSupervisorDto
        {
            Id = supervisor.Id,
            AssociationUserId = supervisor.UserId,
            ProjectId = supervisor.ProjectId,
            AssociationId = associationId,
            ProjectName = supervisor.Project.ProjectNameEn,
            AssociationName = associationId is int id && associationNames.TryGetValue(id, out var name) ? name : null,
            UserName = $"{supervisor.User.FirstNameEn} {supervisor.User.LastNameEn}".Trim(),
            Email = supervisor.User.Email,
            PhoneNumber = supervisor.User.PhoneNumber
        };
    }
}
