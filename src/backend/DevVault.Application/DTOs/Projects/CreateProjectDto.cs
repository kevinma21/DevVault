using System.ComponentModel.DataAnnotations;

namespace DevVault.Application.DTOs.Projects;

public class CreateProjectDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;
}
