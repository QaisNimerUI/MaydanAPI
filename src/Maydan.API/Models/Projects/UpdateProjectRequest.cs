using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Maydan.API.Models.Projects;

// Mirrors CreateProjectRequest, with two differences matching workforcment's projects.service.ts
// update() call exactly: Id travels in the body (PUT /api/Projects, not /api/Projects/{id} — an
// unusual convention, but it's what the frontend already sends), and WorkPermitImage is optional
// since an edit doesn't have to replace the work permit file.
public class UpdateProjectRequest
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProjectNameEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProjectNameAr { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public int ProjectTypeId { get; set; }

    [Required]
    public int ProducerUserId { get; set; }

    [Required]
    public int LocationManagerUserId { get; set; }

    public IFormFile? WorkPermitImage { get; set; }
}
