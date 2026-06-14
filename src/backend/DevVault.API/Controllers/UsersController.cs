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
}
