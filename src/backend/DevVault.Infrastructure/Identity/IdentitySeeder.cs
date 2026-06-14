using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Identity;

public static class IdentitySeeder
{
    // We pass in the UserManager and RoleManager so the seeder can talk to the database
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = { "Administrator", "Developer", "Auditor" };

        foreach (var roleName in roles)
        {
            // If the role doesn't exist, create it
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }
        
        // Create the initial Super Admin account
        var adminEmail = "superAdmin@devvault.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "SuperAdmin123!");

            // Assign the Administrator role to this new user
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}
