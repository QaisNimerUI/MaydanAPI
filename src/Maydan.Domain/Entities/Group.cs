using Maydan.Domain.Common;
using Maydan.Domain.Enums;

namespace Maydan.Domain.Entities;

public class Group : SharedEntities
{
    public int GroupId { get; set; }
    public string GroupNameEn { get; set; } = string.Empty;
    public string GroupNameAr { get; set; } = string.Empty;
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
