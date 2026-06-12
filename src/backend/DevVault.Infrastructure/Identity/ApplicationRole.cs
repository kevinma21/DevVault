using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string description { get; set; } = string.Empty;   
}