using DevVault.Application.DTOs.Auth;
using DevVault.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevVault.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // We inject the interface, NOT the implementation!
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            // Return 401 Unauthorized formatted to your error spec
            return Unauthorized(new 
            { 
                success = false, 
                message = "Invalid Email or Password.", 
                errors = result.Errors
            });
        }

        // Return 200 OK formatted to your success spec
        return Ok(new
        {
            success = true,
            message = "Login successful.",
            data = result.Data
        });
    }
}