using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class UserGroup : SharedEntities
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}
