using Maydan.Domain.Enums;

namespace Maydan.Application.DTOs.UserManagement;

public record PermissionDto(
    int PermissionId,
    string PermissionNameEn,
    string PermissionNameAr,
    string Module);

public record GroupSummaryDto(
    int GroupId,
    string GroupNameEn,
    string GroupNameAr,
    int PermissionCount,
    int UserCount);

public record GroupDetailsDto(
    int GroupId,
    string GroupNameEn,
    string GroupNameAr,
    EntityType EntityType,
    int EntityId,
    List<PermissionDto> Permissions,
    List<UserSummaryDto> Users);

public record UserSummaryDto(
    int UserId,
    string FirstNameEn,
    string LastNameEn,
    string FirstNameAr,
    string LastNameAr,
    string Email,
    string PhoneNumber,
    int RoleId,
    string RoleNameEn,
    string RoleNameAr,
    bool MustResetPassword,
    bool IsActive);

// MAYD-20: real paginated list contract — matches the BRD's own explicit response shape
// (`{ items, totalCount, page, pageSize }`, see Claude outputs/maydan-backend-requirements.md
// section 1.2 in the frontend repo) exactly, camelCase over the wire same as every other DTO here.
public record PagedUsersDto(
    List<UserSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record UserDetailsDto(
    int UserId,
    string FirstNameEn,
    string LastNameEn,
    string FirstNameAr,
    string LastNameAr,
    string Email,
    string PhoneNumber,
    int RoleId,
    string RoleNameEn,
    string RoleNameAr,
    EntityType EntityType,
    int EntityId,
    bool MustResetPassword,
    bool IsActive,
    List<PermissionDto> DirectPermissions,
    List<GroupSummaryDto> Groups,
    List<PermissionDto> EffectivePermissions);

public record PermissionMatrixRoleDto(
    int RoleId,
    string RoleNameEn,
    string RoleNameAr,
    List<int> PermissionIds);

public record PermissionMatrixDto(
    List<PermissionDto> Permissions,
    List<PermissionMatrixRoleDto> Roles);

public class CreateEntityUserDto
{
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string InitialPassword { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
    public List<int> GroupIds { get; set; } = new();
}

public class CreateGroupDto
{
    public string GroupNameEn { get; set; } = string.Empty;
    public string GroupNameAr { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
    public List<int> UserIds { get; set; } = new();
}

public class UpdateGroupDto
{
    public string GroupNameEn { get; set; } = string.Empty;
    public string GroupNameAr { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
    public List<int> UserIds { get; set; } = new();
}

public class UpdateUserPermissionsDto
{
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateUserGroupsDto
{
    public List<int> GroupIds { get; set; } = new();
}
