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

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "User not found in token." });

        var result = await _projectService.GetUserProjectsAsync(userId);

        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to fetch projects.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Projects retrieved successfully.", data = result.Data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "User not found in token." });

        var result = await _projectService.GetProjectByIdAsync(id, userId);

        if (!result.Success)
        {
            // If the service couldn't find it (or the user doesn't own it), return a 404 Not Found
            return NotFound(new { success = false, message = "Project not found.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Project retrieved successfully.", data = result.Data });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "User not found in token." });

        var result = await _projectService.DeleteProjectAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to delete project.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Project deleted successfully.", data = new {} });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, [FromBody] UpdateProjectDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "User not found in token." });

        var result = await _projectService.UpdateProjectAsync(id, request, userId);

        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to update project.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Project updated successfully.", data = result.Data });
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AssignMember(string id, [FromBody] AssignMemberDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _projectService.AssignMemberAsync(id, request, userId);
        if (!result.Success) return BadRequest(new { success = false, message = "Failed to assign member.", errors = result.Errors });

        return Ok(new { success = true, message = "Member assigned successfully." });
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(string id, string memberId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _projectService.RemoveMemberAsync(id, memberId, userId);
        if (!result.Success) return BadRequest(new { success = false, message = "Failed to remove member.", errors = result.Errors });

        return Ok(new { success = true, message = "Member removed successfully." });
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _projectService.GetProjectMembersAsync(id, userId);
        if (!result.Success) return BadRequest(new { success = false, message = "Failed to fetch members.", errors = result.Errors });

        return Ok(new { success = true, message = "Members retrieved successfully.", data = result.Data });
    }
}