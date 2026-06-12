using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    
    public string firstName { get; set; } = string.Empty;
    public string lastName { get; set; } = string.Empty;
    public bool isActive { get; set; } = true; // Default to true so new users can log in
    public DateTime createdAt { get; set; } = DateTime.UtcNow; // Set to current time when user is created
}