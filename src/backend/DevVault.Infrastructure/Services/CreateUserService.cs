using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Users;
using DevVault.Application.Interfaces;
using DevVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DevVault.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService (UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserResult> CreateUserAsync(CreateUserDto request)
    {
        // 1. Verify the RoleId actually exists
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        if (role == null)
        {
            return new UserResult
            {
                Success = false,
                Errors = new[] { "Invalid Role Id Provided." }
            };
        }
        // 2. Create the User entity
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        // 3. Save the user with their temporary password
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new UserResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }
        // 4. Assign the role
        await _userManager.AddToRoleAsync(user, role.Name!);

        // 5. Return the safe response
        return new UserResult
        {
            Success = true,
            Data = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = new List<string> { role.Name! }
            }
        };
    }
}
