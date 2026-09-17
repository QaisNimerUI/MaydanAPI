using Maydan.Application.DTOs.UserManagement;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;

namespace Maydan.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserSummaryDto>> GetUsersAsync(int currentUserId, string? search, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var users = await _unitOfWork.Users.GetByEntityAsync(currentUser.EntityType, currentUser.EntityId, search, cancellationToken);

        return users.Select(MapUserSummary).ToList();
    }

    public async Task<UserDetailsDto> GetUserDetailsAsync(int currentUserId, int userId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var user = await GetScopedUserAsync(userId, currentUser, cancellationToken);

        return MapUserDetails(user);
    }

    public async Task<UserDetailsDto> CreateUserAsync(int currentUserId, CreateEntityUserDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        ValidateUserPayload(dto);

        var email = dto.Email.Trim();
        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("User email already exists.");
        }

        var role = await _unitOfWork.Roles.GetWithPermissionsAsync(dto.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role was not found.");

        var permissionIds = NormalizeIds(dto.PermissionIds);
        await EnsurePermissionsExistAsync(permissionIds, cancellationToken);
        EnsurePermissionsAllowedForRole(role, permissionIds);

        var groupIds = NormalizeIds(dto.GroupIds);
        var groups = await EnsureGroupsInEntityAsync(groupIds, currentUser, cancellationToken);

        var user = new User
        {
            FirstNameEn = dto.FirstNameEn.Trim(),
            LastNameEn = dto.LastNameEn.Trim(),
            FirstNameAr = dto.FirstNameAr.Trim(),
            LastNameAr = dto.LastNameAr.Trim(),
            Email = email,
            PhoneNumber = dto.PhoneNumber.Trim(),
            PasswordHash = _passwordHasher.HashPassword(dto.InitialPassword),
            MustResetPassword = true,
            IsActive = true,
            RoleId = role.RoleId,
            EntityType = currentUser.EntityType,
            EntityId = currentUser.EntityId
        };

        foreach (var permissionId in permissionIds)
        {
            user.UserPermissions.Add(new UserPermission { PermissionId = permissionId, IsActive = true });
        }

        foreach (var group in groups)
        {
            user.UserGroups.Add(new UserGroup { GroupId = group.GroupId });
        }

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdUser = await _unitOfWork.Users.GetDetailsReadOnlyAsync(user.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Created user was not found.");

        return MapUserDetails(createdUser);
    }

    public async Task<List<PermissionDto>> GetAvailablePermissionsAsync(int currentUserId, int? roleId, CancellationToken cancellationToken = default)
    {
        await GetCurrentUserAsync(currentUserId, cancellationToken);

        if (!roleId.HasValue)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllAsync(cancellationToken);
            return allPermissions.Select(MapPermission).ToList();
        }

        var role = await _unitOfWork.Roles.GetWithPermissionsAsync(roleId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Role was not found.");

        return role.RolePermissions
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => MapPermission(rp.Permission))
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionNameEn)
            .ToList();
    }

    public async Task<PermissionMatrixDto> GetPermissionMatrixAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetCurrentUserAsync(currentUserId, cancellationToken);

        var permissions = await _unitOfWork.Permissions.GetAllAsync(cancellationToken);
        var roles = await _unitOfWork.Roles.GetAllWithPermissionsAsync(cancellationToken);

        return new PermissionMatrixDto(
            permissions.Select(MapPermission).ToList(),
            roles.Select(role => new PermissionMatrixRoleDto(
                role.RoleId,
                role.RoleNameEn,
                role.RoleNameAr,
                role.RolePermissions
                    .Where(rp => rp.IsActive && rp.Permission.IsActive)
                    .Select(rp => rp.PermissionId)
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList()))
                .ToList());
    }

    public async Task<List<GroupSummaryDto>> GetGroupsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var groups = await _unitOfWork.Groups.GetByEntityAsync(currentUser.EntityType, currentUser.EntityId, cancellationToken);

        return groups.Select(MapGroupSummary).ToList();
    }

    public async Task<GroupDetailsDto> GetGroupAsync(int currentUserId, int groupId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var group = await GetScopedGroupAsync(groupId, currentUser, cancellationToken);

        return MapGroupDetails(group);
    }

    public async Task<GroupDetailsDto> CreateGroupAsync(int currentUserId, CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        ValidateGroupPayload(dto.GroupNameEn, dto.GroupNameAr);

        if (await _unitOfWork.Groups.NameExistsInEntityAsync(currentUser.EntityType, currentUser.EntityId, dto.GroupNameEn, dto.GroupNameAr, null, cancellationToken))
        {
            throw new InvalidOperationException("Group English or Arabic name already exists in this entity.");
        }

        var permissionIds = NormalizeIds(dto.PermissionIds);
        await EnsurePermissionsExistAsync(permissionIds, cancellationToken);

        var userIds = NormalizeIds(dto.UserIds);
        var users = await EnsureUsersInEntityAsync(userIds, currentUser, cancellationToken);

        var group = new Group
        {
            GroupNameEn = dto.GroupNameEn.Trim(),
            GroupNameAr = dto.GroupNameAr.Trim(),
            EntityType = currentUser.EntityType,
            EntityId = currentUser.EntityId,
            IsActive = true
        };

        foreach (var permissionId in permissionIds)
        {
            group.GroupPermissions.Add(new GroupPermission { PermissionId = permissionId, IsActive = true });
        }

        foreach (var user in users)
        {
            group.UserGroups.Add(new UserGroup { UserId = user.UserId });
        }

        await _unitOfWork.Groups.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetGroupAsync(currentUserId, group.GroupId, cancellationToken);
    }

    public async Task<GroupDetailsDto> UpdateGroupAsync(int currentUserId, int groupId, UpdateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var group = await GetScopedGroupAsync(groupId, currentUser, cancellationToken);
        ValidateGroupPayload(dto.GroupNameEn, dto.GroupNameAr);

        if (await _unitOfWork.Groups.NameExistsInEntityAsync(currentUser.EntityType, currentUser.EntityId, dto.GroupNameEn, dto.GroupNameAr, groupId, cancellationToken))
        {
            throw new InvalidOperationException("Group English or Arabic name already exists in this entity.");
        }

        var permissionIds = NormalizeIds(dto.PermissionIds);
        await EnsurePermissionsExistAsync(permissionIds, cancellationToken);

        var userIds = NormalizeIds(dto.UserIds);
        var users = await EnsureUsersInEntityAsync(userIds, currentUser, cancellationToken);

        group.GroupNameEn = dto.GroupNameEn.Trim();
        group.GroupNameAr = dto.GroupNameAr.Trim();
        SyncGroupPermissions(group, permissionIds);
        SyncGroupUsers(group, users.Select(u => u.UserId).ToList());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetGroupAsync(currentUserId, groupId, cancellationToken);
    }

    public async Task DeleteGroupAsync(int currentUserId, int groupId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var group = await GetScopedGroupAsync(groupId, currentUser, cancellationToken);

        var permissionIds = group.GroupPermissions
            .Where(gp => gp.IsActive && gp.Permission.IsActive)
            .Select(gp => gp.PermissionId)
            .Distinct()
            .ToList();

        foreach (var userGroup in group.UserGroups)
        {
            var existingDirectPermissionIds = userGroup.User.UserPermissions
                .Where(up => up.IsActive)
                .Select(up => up.PermissionId)
                .ToHashSet();

            foreach (var permissionId in permissionIds)
            {
                if (!existingDirectPermissionIds.Contains(permissionId))
                {
                    userGroup.User.UserPermissions.Add(new UserPermission
                    {
                        UserId = userGroup.UserId,
                        PermissionId = permissionId,
                        IsActive = true
                    });
                }
            }
        }

        _unitOfWork.Groups.Remove(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDetailsDto> UpdateDirectPermissionsAsync(int currentUserId, int userId, UpdateUserPermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var user = await GetScopedUserAsync(userId, currentUser, cancellationToken);

        var permissionIds = NormalizeIds(dto.PermissionIds);
        await EnsurePermissionsExistAsync(permissionIds, cancellationToken);
        await EnsurePermissionsAllowedForRoleAsync(user.RoleId, permissionIds, cancellationToken);

        SyncUserPermissions(user, permissionIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserDetailsAsync(currentUserId, userId, cancellationToken);
    }

    public async Task<UserDetailsDto> UpdateGroupsAsync(int currentUserId, int userId, UpdateUserGroupsDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(currentUserId, cancellationToken);
        var user = await GetScopedUserAsync(userId, currentUser, cancellationToken);

        var groupIds = NormalizeIds(dto.GroupIds);
        await EnsureGroupsInEntityAsync(groupIds, currentUser, cancellationToken);

        SyncUserGroups(user, groupIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserDetailsAsync(currentUserId, userId, cancellationToken);
    }

    public async Task<List<PermissionDto>> GetEffectivePermissionsAsync(int currentUserId, int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserDetailsAsync(currentUserId, userId, cancellationToken);

        return user.EffectivePermissions;
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

    private async Task<User> GetScopedUserAsync(int userId, User currentUser, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetDetailsAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User was not found.");

        if (user.EntityType != currentUser.EntityType || user.EntityId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Cannot manage users outside the current entity.");
        }

        return user;
    }

    private async Task<Group> GetScopedGroupAsync(int groupId, User currentUser, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups.GetDetailsAsync(groupId, cancellationToken)
            ?? throw new KeyNotFoundException("Group was not found.");

        if (group.EntityType != currentUser.EntityType || group.EntityId != currentUser.EntityId)
        {
            throw new UnauthorizedAccessException("Cannot manage groups outside the current entity.");
        }

        return group;
    }

    private async Task EnsurePermissionsExistAsync(List<int> permissionIds, CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return;
        }

        var permissions = await _unitOfWork.Permissions.GetByIdsAsync(permissionIds, cancellationToken);
        if (permissions.Count != permissionIds.Count)
        {
            throw new InvalidOperationException("One or more permissions were not found.");
        }
    }

    private async Task EnsurePermissionsAllowedForRoleAsync(int roleId, List<int> permissionIds, CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return;
        }

        var role = await _unitOfWork.Roles.GetWithPermissionsAsync(roleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role was not found.");

        EnsurePermissionsAllowedForRole(role, permissionIds);
    }

    private static void EnsurePermissionsAllowedForRole(Role role, List<int> permissionIds)
    {
        if (permissionIds.Count == 0)
        {
            return;
        }

        var allowedPermissionIds = role.RolePermissions
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        if (permissionIds.Any(permissionId => !allowedPermissionIds.Contains(permissionId)))
        {
            throw new InvalidOperationException("One or more permissions are not available for the selected role.");
        }
    }

    private async Task<List<Group>> EnsureGroupsInEntityAsync(List<int> groupIds, User currentUser, CancellationToken cancellationToken)
    {
        if (groupIds.Count == 0)
        {
            return new List<Group>();
        }

        var groups = await _unitOfWork.Groups.GetByIdsInEntityAsync(groupIds, currentUser.EntityType, currentUser.EntityId, cancellationToken);
        if (groups.Count != groupIds.Count)
        {
            throw new UnauthorizedAccessException("One or more groups were not found in the current entity.");
        }

        return groups;
    }

    private async Task<List<User>> EnsureUsersInEntityAsync(List<int> userIds, User currentUser, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new List<User>();
        }

        var users = await _unitOfWork.Users.GetByIdsInEntityAsync(userIds, currentUser.EntityType, currentUser.EntityId, cancellationToken);
        if (users.Count != userIds.Count)
        {
            throw new UnauthorizedAccessException("One or more users were not found in the current entity.");
        }

        return users;
    }

    private static void ValidateUserPayload(CreateEntityUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstNameEn) ||
            string.IsNullOrWhiteSpace(dto.LastNameEn) ||
            string.IsNullOrWhiteSpace(dto.FirstNameAr) ||
            string.IsNullOrWhiteSpace(dto.LastNameAr) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
            string.IsNullOrWhiteSpace(dto.InitialPassword))
        {
            throw new InvalidOperationException("User mandatory fields are required.");
        }
    }

    private static void ValidateGroupPayload(string groupNameEn, string groupNameAr)
    {
        if (string.IsNullOrWhiteSpace(groupNameEn) || string.IsNullOrWhiteSpace(groupNameAr))
        {
            throw new InvalidOperationException("Group English and Arabic names are required.");
        }
    }

    private static List<int> NormalizeIds(IEnumerable<int> ids) =>
        ids.Where(id => id > 0).Distinct().OrderBy(id => id).ToList();

    private static void SyncUserPermissions(User user, List<int> permissionIds)
    {
        var requestedIds = permissionIds.ToHashSet();
        var existingIds = user.UserPermissions.Select(up => up.PermissionId).ToHashSet();

        foreach (var userPermission in user.UserPermissions.Where(up => !requestedIds.Contains(up.PermissionId)).ToList())
        {
            user.UserPermissions.Remove(userPermission);
        }

        foreach (var permissionId in requestedIds.Except(existingIds))
        {
            user.UserPermissions.Add(new UserPermission
            {
                UserId = user.UserId,
                PermissionId = permissionId,
                IsActive = true
            });
        }
    }

    private static void SyncUserGroups(User user, List<int> groupIds)
    {
        var requestedIds = groupIds.ToHashSet();
        var existingIds = user.UserGroups.Select(ug => ug.GroupId).ToHashSet();

        foreach (var userGroup in user.UserGroups.Where(ug => !requestedIds.Contains(ug.GroupId)).ToList())
        {
            user.UserGroups.Remove(userGroup);
        }

        foreach (var groupId in requestedIds.Except(existingIds))
        {
            user.UserGroups.Add(new UserGroup
            {
                UserId = user.UserId,
                GroupId = groupId
            });
        }
    }

    private static void SyncGroupPermissions(Group group, List<int> permissionIds)
    {
        var requestedIds = permissionIds.ToHashSet();
        var existingIds = group.GroupPermissions.Select(gp => gp.PermissionId).ToHashSet();

        foreach (var groupPermission in group.GroupPermissions.Where(gp => !requestedIds.Contains(gp.PermissionId)).ToList())
        {
            group.GroupPermissions.Remove(groupPermission);
        }

        foreach (var permissionId in requestedIds.Except(existingIds))
        {
            group.GroupPermissions.Add(new GroupPermission
            {
                GroupId = group.GroupId,
                PermissionId = permissionId,
                IsActive = true
            });
        }
    }

    private static void SyncGroupUsers(Group group, List<int> userIds)
    {
        var requestedIds = userIds.ToHashSet();
        var existingIds = group.UserGroups.Select(ug => ug.UserId).ToHashSet();

        foreach (var userGroup in group.UserGroups.Where(ug => !requestedIds.Contains(ug.UserId)).ToList())
        {
            group.UserGroups.Remove(userGroup);
        }

        foreach (var userId in requestedIds.Except(existingIds))
        {
            group.UserGroups.Add(new UserGroup
            {
                UserId = userId,
                GroupId = group.GroupId
            });
        }
    }

    private static UserSummaryDto MapUserSummary(User user) =>
        new(
            user.UserId,
            user.FirstNameEn,
            user.LastNameEn,
            user.FirstNameAr,
            user.LastNameAr,
            user.Email,
            user.PhoneNumber,
            user.RoleId,
            user.Role.RoleNameEn,
            user.Role.RoleNameAr,
            user.MustResetPassword,
            user.IsActive);

    private static UserDetailsDto MapUserDetails(User user)
    {
        var directPermissions = user.UserPermissions
            .Where(up => up.IsActive && up.Permission.IsActive)
            .Select(up => up.Permission)
            .DistinctBy(p => p.PermissionId)
            .Select(MapPermission)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionNameEn)
            .ToList();

        var groupPermissions = user.UserGroups
            .Select(ug => ug.Group)
            .Where(g => g.IsActive)
            .SelectMany(g => g.GroupPermissions)
            .Where(gp => gp.IsActive && gp.Permission.IsActive)
            .Select(gp => gp.Permission);

        var effectivePermissions = user.UserPermissions
            .Where(up => up.IsActive && up.Permission.IsActive)
            .Select(up => up.Permission)
            .Concat(groupPermissions)
            .DistinctBy(p => p.PermissionId)
            .Select(MapPermission)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionNameEn)
            .ToList();

        return new UserDetailsDto(
            user.UserId,
            user.FirstNameEn,
            user.LastNameEn,
            user.FirstNameAr,
            user.LastNameAr,
            user.Email,
            user.PhoneNumber,
            user.RoleId,
            user.Role.RoleNameEn,
            user.Role.RoleNameAr,
            user.EntityType,
            user.EntityId,
            user.MustResetPassword,
            user.IsActive,
            directPermissions,
            user.UserGroups.Select(ug => MapGroupSummary(ug.Group)).ToList(),
            effectivePermissions);
    }

    private static GroupSummaryDto MapGroupSummary(Group group) =>
        new(
            group.GroupId,
            group.GroupNameEn,
            group.GroupNameAr,
            group.GroupPermissions.Count(gp => gp.IsActive),
            group.UserGroups.Count);

    private static GroupDetailsDto MapGroupDetails(Group group) =>
        new(
            group.GroupId,
            group.GroupNameEn,
            group.GroupNameAr,
            group.EntityType,
            group.EntityId,
            group.GroupPermissions
                .Where(gp => gp.IsActive && gp.Permission.IsActive)
                .Select(gp => MapPermission(gp.Permission))
                .OrderBy(p => p.Module)
                .ThenBy(p => p.PermissionNameEn)
                .ToList(),
            group.UserGroups
                .Select(ug => MapUserSummary(ug.User))
                .OrderBy(u => u.FirstNameEn)
                .ThenBy(u => u.LastNameEn)
                .ToList());

    private static PermissionDto MapPermission(Permission permission) =>
        new(
            permission.PermissionId,
            permission.PermissionNameEn,
            permission.PermissionNameAr,
            permission.Module);
}
