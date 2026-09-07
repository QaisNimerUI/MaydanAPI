using Maydan.Domain.Common;
using Maydan.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maydan.Domain.Entities;

public class User:SharedEntities
{
    public int UserId { get; set; }
    public string FirstNameAr { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool MustResetPassword { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    // Tells which real-world entity this user belongs to; EntityId is a soft FK
    // (no single target table) to Association.Id / ProductionCompany.Id / etc.
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
