using DevVault.Application.DTOs.Auth;
using DevVault.Application.Interfaces;
using DevVault.Infrastructure.Security;
using DevVault.Infrastructure.Identity;
using DevVault.Application.DTOs.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevVault.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger; // Added <AuthService>
    private readonly IAuditService _auditService;


    public AuthService(UserManager<ApplicationUser> userManager, JwtService jwtService, IConfiguration configuration, ILogger<AuthService> logger, IAuditService auditService) // FIX 1: Added <AuthService>
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new Result<LoginResponseDto>
                { 
                    Success = false, 
                    Errors = new[] { "Invalid email or password" }
                };
            }

            // Check if the Administrator has deactivated this account!
            if (!user.IsActive)
            {
                return new Result<LoginResponseDto>
                {
                    Success = false,
                    Errors = new[] { "This account has been deactivated. Please contact your administrator." }
                };
            }

            await _auditService.LogActionAsync(
                userId: user.Id, 
                entityType: "Authentication", 
                entityId: user.Id, 
                action: "Login", 
                details: "User successfully authenticated."
            );

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(user, roles);
            var expireDays = Convert.ToDouble(_configuration["Jwt:ExpireDays"]);

            var response = new LoginResponseDto
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expireDays)
            };

            return new Result<LoginResponseDto>
            {
                Success = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A system error occurred during user login attempt for {Email}.", request.Email);
            return new Result<LoginResponseDto> 
            { 
                Success = false, 
                Errors = new[] { "An unexpected error occurred during login. Please try again later." } 
            };
        }
    }
}