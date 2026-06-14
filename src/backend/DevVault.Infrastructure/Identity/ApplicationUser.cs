using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true; // Default to true so new users can log in
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Set to current time when user is created
}