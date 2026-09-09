//using Maydan.Application.DTOs.Projects;
//using Maydan.Application.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using Maydan.API.Models.Projects;

//namespace Maydan.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize]
//public class ProjectsController : ControllerBase
//{
//    private readonly IProjectService _projectService;

//    public ProjectsController(IProjectService projectService)
//    {
//        _projectService = projectService;
//    }

//    [HttpPost]
//    public async Task<IActionResult> Create(
//    [FromForm] CreateProjectRequest request,
//    CancellationToken cancellationToken)
//    {
//        var result = await _projectService.CreateAsync(
//            request,
//            cancellationToken);

//        return Ok(result);
//    }
//}