using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DevVault.Application.DTOs.Projects;
using DevVault.Application.Interfaces;

namespace DevVault.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController (IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject ([FromBody] CreateProjectDto request)
    {
        // 1. grab user authorization from their JWT token
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. check the authorization
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "User identifier not found in token."
            });
        }

        // 3. pass the Id to create project
        var result = await _projectService.CreateProjectAsync(request, userId);

        // 4. fallback error if the operation not success
        if (!result.Success)
        {
            return BadRequest(new
            {
               success = false,
               message = "Failed to Create project.",
               erorrs = result.Errors 
            });
        }

        // 5. return Data if success
        return Ok(new
        {
            success = true,
            message = "Project created successfuly.",
            data = result.Data
        });
    }
}