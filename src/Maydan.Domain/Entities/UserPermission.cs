using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class UserPermission 
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
