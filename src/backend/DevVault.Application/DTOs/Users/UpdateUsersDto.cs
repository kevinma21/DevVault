using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
