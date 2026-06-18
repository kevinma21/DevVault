using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Common;

namespace DevVault.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto request);
}
