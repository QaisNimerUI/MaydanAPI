using Maydan.Application.DTOs.Associations;

namespace Maydan.Application.Interfaces;

// Association Management, Phase 2a (MAYD-4/MAYD-40..54) — see AssociationService's own header
// comment for the full "what already existed vs. what this pass built" story. Admin-creation for a
// new association (EntityOnboardingController's real associations/without-admin +
// associations/{id}/admin) is a separate, already-built flow and stays out of this interface.
public interface IAssociationService
{
    Task<List<AssociationDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<AssociationDto> GetByIdAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default);
    Task<List<AssociationDto>> SearchByNameAsync(int currentUserId, string name, CancellationToken cancellationToken = default);
    Task<List<AssociationDto>> GetOrderedByWorkersCountAsync(int currentUserId, bool ascending, CancellationToken cancellationToken = default);
    Task<List<AssociationDto>> GetDeletedAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<List<AssociationDto>> SearchDeletedByNameAsync(int currentUserId, string name, CancellationToken cancellationToken = default);

    Task<AssociationDto> CreateAsync(int currentUserId, CreateAssociationDto dto, CancellationToken cancellationToken = default);
    Task<AssociationDto> UpdateAsync(int currentUserId, UpdateAssociationDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default);
    Task<AssociationDto> RestoreAsync(int currentUserId, int associationId, CancellationToken cancellationToken = default);
}
