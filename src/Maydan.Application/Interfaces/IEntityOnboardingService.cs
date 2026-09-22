using Maydan.Application.DTOs.Onboarding;
using Maydan.Application.DTOs.UserManagement;

namespace Maydan.Application.Interfaces;

public interface IEntityOnboardingService
{
    Task<UserDetailsDto> OnboardAssociationAdminAsync(int currentUserId, int associationId, OnboardAssociationAdminDto dto, CancellationToken cancellationToken = default);
    Task<List<AssociationWithoutAdminDto>> GetAssociationsWithoutAdminAsync(int currentUserId, CancellationToken cancellationToken = default);
}
