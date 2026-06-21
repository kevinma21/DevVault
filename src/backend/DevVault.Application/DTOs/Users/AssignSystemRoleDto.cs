using System.ComponentModel.DataAnnotations;

public class AssignSystemRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
