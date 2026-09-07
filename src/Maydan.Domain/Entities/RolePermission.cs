using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class RolePermission 
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
