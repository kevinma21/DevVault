using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Projects;
using DevVault.Application.Interfaces;
using DevVault.Domain.Entities;
using DevVault.Infrastructure.Persistence;
using Microsoft.Extensions.Logging; // 1. Add this using statement!

namespace DevVault.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly DevVaultDbContext _context;
    private readonly ILogger<ProjectService> _logger; // 2. Declare the logger

    // 3. Inject the logger into the constructor
    public ProjectService(DevVaultDbContext context, ILogger<ProjectService> logger) 
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto request, string userId)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);

        try
        {
            await _context.SaveChangesAsync();
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
}