namespace Maydan.Application.DTOs.Projects;

public class UpdateProjectDto
{
    public string ProjectNameEn { get; set; } = string.Empty;
    public string ProjectNameAr { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ProjectTypeId { get; set; }

    public int ProducerUserId { get; set; }

    public int LocationManagerUserId { get; set; }

    // Null/empty means "no new file uploaded" — ProjectService.UpdateAsync keeps the project's
    // existing WorkPermitImagePath in that case, since re-uploading the work permit on every
    // unrelated edit isn't a reasonable UX requirement.
    public string? WorkPermitImagePath { get; set; }
}
