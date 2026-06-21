using DevVault.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevVault.Infrastructure.Identity;

namespace DevVault.Infrastructure.Persistence;

public class DevVaultDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DevVaultDbContext(DbContextOptions<DevVaultDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Secret> Secrets { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.ProjectId, pm.UserId });

        // configure relationship
        builder.Entity<Project>()
            .HasMany(p => p.Secrets)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            // If a Project is deleted, automatically wipe all its Secrets from the database
            .OnDelete(DeleteBehavior.Cascade);
    }
}