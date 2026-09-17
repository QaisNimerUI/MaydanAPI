using Maydan.Application.DTOs.UserManagement;

namespace Maydan.Application.Interfaces;

public interface IUserManagementService
{
    Task<List<UserSummaryDto>> GetUsersAsync(int currentUserId, string? search, CancellationToken cancellationToken = default);
    Task<UserDetailsDto> GetUserDetailsAsync(int currentUserId, int userId, CancellationToken cancellationToken = default);
    Task<UserDetailsDto> CreateUserAsync(int currentUserId, CreateEntityUserDto dto, CancellationToken cancellationToken = default);

    Task<List<PermissionDto>> GetAvailablePermissionsAsync(int currentUserId, int? roleId, CancellationToken cancellationToken = default);
    Task<PermissionMatrixDto> GetPermissionMatrixAsync(int currentUserId, CancellationToken cancellationToken = default);

    Task<List<GroupSummaryDto>> GetGroupsAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<GroupDetailsDto> GetGroupAsync(int currentUserId, int groupId, CancellationToken cancellationToken = default);
    Task<GroupDetailsDto> CreateGroupAsync(int currentUserId, CreateGroupDto dto, CancellationToken cancellationToken = default);
    Task<GroupDetailsDto> UpdateGroupAsync(int currentUserId, int groupId, UpdateGroupDto dto, CancellationToken cancellationToken = default);
    Task DeleteGroupAsync(int currentUserId, int groupId, CancellationToken cancellationToken = default);

    Task<UserDetailsDto> UpdateDirectPermissionsAsync(int currentUserId, int userId, UpdateUserPermissionsDto dto, CancellationToken cancellationToken = default);
    Task<UserDetailsDto> UpdateGroupsAsync(int currentUserId, int userId, UpdateUserGroupsDto dto, CancellationToken cancellationToken = default);
    Task<List<PermissionDto>> GetEffectivePermissionsAsync(int currentUserId, int userId, CancellationToken cancellationToken = default);
}
