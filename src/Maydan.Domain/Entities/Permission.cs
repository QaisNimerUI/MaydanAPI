using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Permission : SharedEntities
{
    public int PermissionId { get; set; }
    public string PermissionNameEn { get; set; } = string.Empty;
    public string PermissionNameAr { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
