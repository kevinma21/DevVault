using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Projects;
using DevVault.Application.Interfaces;
using DevVault.Domain.Entities;
using DevVault.Infrastructure.Persistence;
using DevVault.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly DevVaultDbContext _context;
    private readonly ILogger<ProjectService> _logger; // 2. Declare the logger
    private readonly UserManager<ApplicationUser> _userManager;

    // 3. Inject the logger into the constructor
    public ProjectService(DevVaultDbContext context, ILogger<ProjectService> logger, UserManager<ApplicationUser> userManager) 
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto request, string userId)
    {
        try
        {   
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

             var responseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt
            };

            return new Result<ProjectResponseDto>
            {
                Success = true,
                Data = responseDto
            };
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "A database error occurred while user {UserId} was creating a project.", userId);
            return new Result<ProjectResponseDto>
            {
                Success = false,
                Errors = new[] { "An unexpected system error occurred while creating the project. Please try again later." }
            };
        }
    }

    public async Task<Result<IEnumerable<ProjectResponseDto>>> GetUserProjectsAsync(string userId)
    {
        try
        {
            // SECURITY: Only fetch projects where the OwnerId matches the logged-in user
            var projects = await _context.Projects
                .Where(p => p.OwnerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OwnerId = p.OwnerId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return new Result<IEnumerable<ProjectResponseDto>> { Success = true, Data = projects };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error fetching projects for user {UserId}.", userId);
            return new Result<IEnumerable<ProjectResponseDto>> { Success = false, Errors = new[] { "An error occurred while fetching your projects." } };
        }
    }

    public async Task<Result<ProjectResponseDto>> GetProjectByIdAsync(string projectId, string userId)
    {
        try
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

            if (project == null)
            {
                return new Result<ProjectResponseDto> { Success = false, Errors = new[] { "Project not found or access denied." } };
            }

            return new Result<ProjectResponseDto>
            {
                Success = true,
                Data = new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    OwnerId = project.OwnerId,
                    CreatedAt = project.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error fetching project {ProjectId} for user {UserId}.", projectId, userId);
            return new Result<ProjectResponseDto> { Success = false, Errors = new[] { "An error occurred while fetching the project." } };
        }
    }

    public async Task<Result<bool>> DeleteProjectAsync(string projectId, string userId)
    {
        try
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

            if (project == null)
            {
                return new Result<bool> { Success = false, Errors = new[] { "Project not found or access denied." } };
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return new Result<bool> { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error deleting project {ProjectId} for user {UserId}.", projectId, userId);
            return new Result<bool> { Success = false, Errors = new[] { "An error occurred while deleting the project." } };
        }
    }
    public async Task<Result<ProjectResponseDto>> UpdateProjectAsync(string projectId, UpdateProjectDto request, string userId)
    {
        try
        {
            // SECURITY: Ensure the project exists AND belongs to the user making the request
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

            if (project == null)
            {
                return new Result<ProjectResponseDto> { Success = false, Errors = new[] { "Project not found or access denied." } };
            }

            // Apply the updates
            project.Name = request.Name;
            project.Description = request.Description;

            await _context.SaveChangesAsync();

            return new Result<ProjectResponseDto>
            {
                Success = true,
                Data = new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    OwnerId = project.OwnerId,
                    CreatedAt = project.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error updating project {ProjectId} for user {UserId}.", projectId, userId);
            return new Result<ProjectResponseDto> { Success = false, Errors = new[] { "An error occurred while updating the project." } };
        }
    }

    public async Task<Result<bool>> AssignMemberAsync (string projectId, AssignMemberDto request, string ownerId)
    {   
        try
        {
            // 1. Verify the requester is the actual OWNER of the project
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == ownerId);
            if (project == null) return new Result<bool> { Success = false, Errors = new[] { "Project not found or access denied." } };

            // 2. Find the user they are trying to invite
            var userToInvite = await _userManager.FindByEmailAsync(request.Email);
            if (userToInvite == null) return new Result<bool> { Success = false, Errors = new[] { "User with that email does not exist." }};

            // 3. Prevent owners from inviting themselves
            if (userToInvite.Id == ownerId) return new Result<bool> { Success = false, Errors = new[] { "You cannot invite yourself." }};

            // 4. Check if they are already a member
            var existingMember = await _context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userToInvite.Id);
            if (existingMember != null) return new Result<bool> { Success = false, Errors = new[] { "User is already member of this project." }};

            // 5. Add them to the project
            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userToInvite.Id,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow
            };

            await _context.ProjectMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            return new Result<bool> 
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning member to project {ProjectId}.", projectId);
            return new Result<bool> { Success = false, Errors = new[] { "An unexpected error occurred." } };
        }
    }

    public async Task<Result<bool>> RemoveMemberAsync (string projectId, string memberId, string ownerId)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return new Result<bool> { Success = false, Errors = new[] { "Project not found or access denied." } };
            
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == memberId);
            if (member == null) return new Result<bool> { Success = false, Errors = new[] { "Member not found in this project." } };

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();    

            return new Result<bool>
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from project {ProjectId}.", projectId);
            return new Result<bool> { Success = false, Errors = new[] { "An unexpected error occurred." } };
        }
    }

    public async Task<Result<IEnumerable<ProjectMemberResponseDto>>> GetProjectMembersAsync (string projectId, string userId)
    {
        try
        {
            // Verify the requester is either the Owner OR an existing Member
            var isOwner = await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
            var isMember = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (!isOwner && !isMember) return new Result<IEnumerable<ProjectMemberResponseDto>> { Success = false, Errors = new[] { "Access denied." } };

            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => new ProjectMemberResponseDto
                {
                    UserId = pm.UserId,
                    Role = pm.Role,
                    JoinedAt = pm.JoinedAt
                    // Email would typically be fetched via a join with Identity, 
                    // or by querying the UserManager directly afterwards.
                })
                .ToListAsync();

            return new Result<IEnumerable<ProjectMemberResponseDto>> { Success = true, Data = members };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching members for project {ProjectId}.", projectId);
            return new Result<IEnumerable<ProjectMemberResponseDto>> { Success = false, Errors = new[] { "An unexpected error occurred." } };
        }
    }
}
