using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevVault.Infrastructure.Identity;

namespace DevVault.Infrastructure.Persistence;

public class DevVaultDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DevVaultDbContext(DbContextOptions<DevVaultDbContext> options) : base(options)
    {
    }
}