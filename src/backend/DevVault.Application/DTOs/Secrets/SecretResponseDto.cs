namespace DevVault.Application.DTOs.Secrets;

public class SecretResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    // We only populate this when they explicitly ask to reveal it!
    public string? DecryptedValue { get; set; }

    public DateTime CreatedAt { get; set; }
}
