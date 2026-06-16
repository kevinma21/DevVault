using DevVault.Application.Interfaces;
using DevVault.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevVault.Application.DTOs.Auth;

namespace DevVault.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
// THIS is what locks the entire controller down
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
        if (!result.Success)
        {
            return BadRequest(new
            {
                success = false,
                message = "Failed to create user.",
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            message = "User created successfully.",
            data = result.Data
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Don't allow a malicious user to request a million records at once
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var pagedResult = await _userService.GetUsersAsync(pageNumber, pageSize);

        return Ok(new
        {
            success = true,
            message = "Operation complete successfully.",
            data = pagedResult
        });
    }

    [HttpGet("{id}")]
    public async  Task<IActionResult> GetUserByAsync ([FromQuery] string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Operation completed succussfully.",
            data = user
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser (string id, [FromBody] UpdateUserDto request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        if(!result.Success)
        {
            return BadRequest(new
            {
               success = false,
               message = "Failed to update user.",
               errors = result.Errors 
            });
        }

        return Ok(new
        {
           success = true,
           message = "User updated successfully.",
           data = result.Data
        });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var result = await _userService.DeactivateUserAsync(id);

        if (!result.Success)
        {
            return BadRequest(new
            {
                success = false,
                message = "Failed to deactivate user.",
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            message = "User deactivated successfully.",
            data = new {}
        });
    }
}
