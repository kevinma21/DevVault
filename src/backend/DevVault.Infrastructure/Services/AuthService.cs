using DevVault.Application.DTOs.Auth;
using DevVault.Application.Interfaces;
using DevVault.Infrastructure.Security;
using DevVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace DevVault.Infrastructure.Services;

// This class implements the interface defined in the Application layer
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, JwtService jwtService, IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResult> LoginAsync(LoginDto request)
    {
        // Find the user by their email
        var user = await _userManager.FindByEmailAsync(request.Email);

        // If user doesn't exist, or password doesn't match, return an error.
        // We use the exact same error message for both to prevent username enumeration attacks.
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new LoginResult 
            { 
                Success = false, 
                ErrorMessage = "Invalid email or password." 
            };
        }
        // Fetch the user's roles from the database
        var roles = await _userManager.GetRolesAsync(user);

        // Generate the token
        var token = _jwtService.GenerateToken(user, roles);
        var expireDays = Convert.ToDouble(_configuration["Jwt:ExpireDays"]);

        return new LoginResult
        {
            Success = true,
            Data = new LoginResponseDto
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expireDays)
            }
        };
    }
}