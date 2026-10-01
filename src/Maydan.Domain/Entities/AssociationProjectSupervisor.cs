using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

// Association Management, Phase 2e: links a Project to the real User assigned to supervise it.
// UserId always points at a real User.UserId (the actual primary key configured on User — see
// UserConfiguration.cs's HasKey(u => u.UserId), not the inherited SharedEntities.Id) — specifically
// an Association-role user (EntityType.Association), the same real rows Phase 2b's own EntityId
// wiring made properly scoped. No separate "AssociationUser" entity is involved here at all.
public class AssociationProjectSupervisor : SharedEntities
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
