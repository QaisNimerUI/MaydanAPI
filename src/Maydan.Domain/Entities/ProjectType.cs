using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class ProjectType : SharedEntities
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; }
        = new List<Project>();
}