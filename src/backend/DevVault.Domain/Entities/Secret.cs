namespace DevVault.Domain.Entities;

public class Secret
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // e.g., "AWS_ACCESS_KEY"
    public string Key { get; set; } = string.Empty;
    // The mathematically encrypted cipher text
    public string EncryptedValue { get; set; } = string.Empty;
    // Which project does this belong to?
    public string ProjectId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: Connects back to the parent Project
    public Project? Project { get; set; }
}