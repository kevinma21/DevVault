using System.ComponentModel.DataAnnotations;

namespace DevVault.Application.DTOs.Secrets;

public class CreateSecretDto
{
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}
