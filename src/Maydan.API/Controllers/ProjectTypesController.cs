using Maydan.Application.DTOs.Projects;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maydan.API.Controllers;

// Projects audit follow-up: project-form.component.ts's projectType dropdown was hardcoded to
// ['Productions', 'Tourism', 'Events'] — a fictional list, never fetched from anywhere. The real
// catalog is ProjectTypeSeed.cs's 15 seeded rows. This is the endpoint the frontend should read
// that catalog from instead.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectTypesController : ApiControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectTypesController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _projectService.GetProjectTypesAsync(cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}
