using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Projects;

namespace DevVault.Application.Interfaces;

public interface IProjectService
{
    Task<Result<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto request, string userId);
    // Fetch all projects owned by this specific user
    Task<Result<IEnumerable<ProjectResponseDto>>> GetUserProjectsAsync(string userId);
    // Fetch a single project, verifying ownership
    Task<Result<ProjectResponseDto>> GetProjectByIdAsync(string projectId, string userId);
    // Delete a project (and thanks to EF Core Cascade Delete, all its secrets too!)
    Task<Result<bool>> DeleteProjectAsync(string projectId, string userId);
    // Update an existing project
    Task<Result<ProjectResponseDto>> UpdateProjectAsync(string projectId, UpdateProjectDto request, string ownerId);

    // Team Management
    Task<Result<bool>> AssignMemberAsync(string projectId, AssignMemberDto request, string ownerId);
    Task<Result<bool>> RemoveMemberAsync(string projectId, string memberId, string ownerId);
    Task<Result<IEnumerable<ProjectMemberResponseDto>>> GetProjectMembersAsync(string projectId, string ownerId);
}