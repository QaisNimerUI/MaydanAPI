using Maydan.Domain.Common;

namespace Maydan.Domain.Entities;

public class ProductionCompany : SharedEntities
{
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsSelfRegistered { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
