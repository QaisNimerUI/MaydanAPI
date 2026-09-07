using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class Project : SharedEntities
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int ProductionCompanyId { get; set; }
    public ProductionCompany ProductionCompany { get; set; } = null!;

    // Deliberately excluded (section: "متعمّد الاستبعاد"): Association link, ProjectLocation,
    // and any Status/Approval field — deferred with the Service Request system.
}
