namespace DevVault.Domain.Entities;

public class ProjectMember
{
    public string ProjectId { get; set; } = string.Empty;
    public Project Project { get; set; } = null!;

    // We store the UserId as a string to link to the Identity User
    public string UserId { get; set; } = string.Empty;
    
    // e.g., "Admin", "Contributor", "Viewer"
    public string Role { get; set; } = "Viewer"; 
    
    public DateTime JoinedAt { get; set; }
}