using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Common;
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

    public async Task<PagedResponse<UserResponseDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        // 1. Get the total count of users BEFORE applying limits (so the frontend knows how many pages exist)
        var totalCount = _userManager.Users.Count();
        // 2. Apply Skip and Take at the database level!
        var users = _userManager.Users
            .Skip((pageNumber - 1) *  pageSize)
            .Take(pageSize)
            .ToList();

        var userResponses = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userResponses.Add(new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    Roles = roles.ToList()
            });
        }

        // 3. Wrap the result in your new generic response    
        return new PagedResponse<UserResponseDto>
        {
            Items = userResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync (string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        };
    }

    public async Task<UserResult> UpdateUserAsync(string id, UpdateUserDto request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new UserResult
            {
                Success = false,
                Errors = new[] { "User not Found." }
            };
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new UserResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

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
                Roles = roles.ToList()
            }
        };
    }

    public async Task<UserResult> DeactivateUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new UserResult
            {
                Success = false,
                Errors = new[] { "User not found." }
            };
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new UserResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        return new UserResult
        {
            Success = true
        };
    }
}
