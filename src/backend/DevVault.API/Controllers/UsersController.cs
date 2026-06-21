using DevVault.Application.Interfaces;
using DevVault.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevVault.Application.DTOs.Auth;

namespace DevVault.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        // The Controller only checks the Success flag!
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to create user.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "User created successfully.", data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize > 100) pageSize = 100;

        var result = await _userService.GetUsersAsync(pageNumber, pageSize);

        // UPDATE: Now we check the Result wrapper instead of blindly assuming it worked
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to fetch users.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Operation complete successfully.", data = result.Data });
    }

    [HttpGet("{id}")]
    // UPDATE: Removed [FromQuery]. Because {id} is in the route above, ASP.NET finds it automatically!
    public async Task<IActionResult> GetUserById(string id) 
    {
        var result = await _userService.GetUserByIdAsync(id);
        
        // UPDATE: Check the Result wrapper instead of checking for `null`
        if (!result.Success)
        {
            // If the service specifically told us the user wasn't found, we can return a 404
            if (result.Errors.Contains("User not found."))
            {
                return NotFound(new { success = false, message = "User not found.", errors = result.Errors });
            }

            return BadRequest(new { success = false, message = "Failed to fetch user.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "Operation completed successfully.", data = result.Data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser (string id, [FromBody] UpdateUserDto request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        
        if(!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to update user.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "User updated successfully.", data = result.Data });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var result = await _userService.DeactivateUserAsync(id);

        if (!result.Success)
        {
            return BadRequest(new { success = false, message = "Failed to deactivate user.", errors = result.Errors });
        }

        return Ok(new { success = true, message = "User deactivated successfully.", data = new {} });
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignSystemRoleDto request)
    {
        var result = await _userService.AssignSystemRoleAsync(id, request.Role);
        if (!result.Success)
        {
            return BadRequest(new
            {
                success = false,
                message = "Failed to assign role.",
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true, 
            message = "User deactivated successfully."
        });
    }
}