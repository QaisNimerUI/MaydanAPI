namespace Maydan.Application.DTOs.Projects;

// Matches workforcment's projects.service.ts getAll()'s ProjectsQuery exactly — field for field,
// same names — so ProjectsController's [FromQuery] binding needs no translation layer.
public class ProjectQueryDto
{
    public bool IsDeleted { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchByProductionCompanyName { get; set; }
    public int? SearchByProductionCompanyId { get; set; }
}
