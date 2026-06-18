using Microsoft.Win32.SafeHandles;

namespace DevVault.Domain.Entities;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Secret> Secrets { get; set; } = new List<Secret>();
}
