using System.ComponentModel.DataAnnotations;

namespace DevVault.Application.DTOs.Projects;

public class AssignMemberDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Viewer";
}