using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Users;
using DevVault.Application.Interfaces;
using DevVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DevVault.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<UserResponseDto>> CreateUserAsync(CreateUserDto request)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                return new Result<UserResponseDto> { Success = false, Errors = new[] { "Invalid Role Id Provided." } };
            }
            
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new Result<UserResponseDto> { Success = false, Errors = result.Errors.Select(e => e.Description) };
            }
            
            await _userManager.AddToRoleAsync(user, role.Name!);

            return new Result<UserResponseDto>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "A database error occurred while creating a new user.");
            return new Result<UserResponseDto> { Success = false, Errors = new[] { "An unexpected system error occurred while creating the user." } };
        }
    }

    public async Task<Result<PagedResponse<UserResponseDto>>> GetUsersAsync(int pageNumber, int pageSize)
    {
        try
        {
            var totalCount = _userManager.Users.Count();
            
            var users = _userManager.Users
                .Skip((pageNumber - 1) * pageSize)
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

            var response = new PagedResponse<UserResponseDto>
            {
                Items = userResponses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new Result<PagedResponse<UserResponseDto>> { Success = true, Data = response };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A database error occurred while getting users data.");
            return new Result<PagedResponse<UserResponseDto>> { Success = false, Errors = new[] { "An unexpected system error occurred while getting the users. Please try again later." } };
        }
    }

    public async Task<Result<UserResponseDto>> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new Result<UserResponseDto> { Success = false, Errors = new[] { "User not found." } };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            };

            return new Result<UserResponseDto> { Success = true, Data = userResponse };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A database error occurred while fetching user {UserId}.", id);
            return new Result<UserResponseDto> { Success = false, Errors = new[] { "An unexpected system error occurred while fetching the user." } };
        }
    }

    public async Task<Result<UserResponseDto>> UpdateUserAsync(string id, UpdateUserDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new Result<UserResponseDto> { Success = false, Errors = new[] { "User not Found." } };
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new Result<UserResponseDto> { Success = false, Errors = result.Errors.Select(e => e.Description) };
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new Result<UserResponseDto>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "A database error occurred while updating user {UserId}.", id);
            return new Result<UserResponseDto> { Success = false, Errors = new[] { "An unexpected system error occurred while updating the user." } };
        }
    }

    public async Task<Result<bool>> DeactivateUserAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new Result<bool> { Success = false, Errors = new[] { "User not found." } };
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new Result<bool> { Success = false, Errors = result.Errors.Select(e => e.Description) };
            }

            return new Result<bool> { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A database error occurred while deactivating user {UserId}.", id);
            return new Result<bool> { Success = false, Errors = new[] { "An unexpected system error occurred while deactivating the user." } };
        }
    }

    public async Task<Result<bool>> AssignSystemRoleAsync (string targetUserId, string role)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) 
            {
                return new Result<bool> { Success = false, Errors = new[] { "User not found." } };
            }

            // 1. Remove all existing roles from the user
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // 2. Assign the new role
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return new Result<bool> 
                { 
                    Success = false, 
                    Errors = addResult.Errors.Select(e => e.Description).ToArray() 
                };
            }

            return new Result<bool> { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning system role to user {UserId}.", targetUserId);
            return new Result<bool> { Success = false, Errors = new[] { "An unexpected error occurred." }};
        }
    }
}