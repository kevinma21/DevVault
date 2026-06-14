using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;   
}