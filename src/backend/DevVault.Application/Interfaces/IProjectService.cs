using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Projects;

namespace DevVault.Application.Interfaces;

public interface IProjectService
{
    Task<Result<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto request, string userId);
}
