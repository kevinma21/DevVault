using DevVault.Application.DTOs.Auth;

namespace DevVault.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginDto request);
}
