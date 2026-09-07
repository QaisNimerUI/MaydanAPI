using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Role : SharedEntities
{
    public int RoleId { get; set; }
    public string RoleNameAr { get; set; } = string.Empty;
    public string RoleNameEn { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
