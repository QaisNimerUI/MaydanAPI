using Maydan.Application.Interfaces;

namespace Maydan.Application.Services;

public class SystemConfigurationGateService : ISystemConfigurationGateService
{
    // "Super admin" (MAYD-133's own wording) maps to RoleId 1 / EntityType.BaytAlUrdon — confirmed
    // against real seed data (RoleSeedConfiguration.cs, UserSeedConfiguration.cs) and, more
    // importantly, against real code: UserManagementService.EnsureSameEntityCreation rejects any
    // attempt to create a user whose RoleId differs from its creator's own RoleId, which means
    // EVERY user ever created within the Bayt-AlUrdon entity necessarily has RoleId == 1 — the two
    // checks are provably equivalent for every real/reachable user, not just the one seeded row.
    // There is no distinct "Super Admin" concept anywhere in this codebase separate from this role.
    private const int SuperAdminRoleId = 1;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemConfigurationService _systemConfigurationService;

    public SystemConfigurationGateService(IUnitOfWork unitOfWork, ISystemConfigurationService systemConfigurationService)
    {
        _unitOfWork = unitOfWork;
        _systemConfigurationService = systemConfigurationService;
    }

    public async Task<bool> ShouldBlockAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser is null || !currentUser.IsActive || currentUser.RoleId != SuperAdminRoleId)
        {
            return false;
        }

        return !await _systemConfigurationService.IsConfiguredAsync(cancellationToken);
    }
}
