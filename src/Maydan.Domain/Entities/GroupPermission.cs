using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class GroupPermission 
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
