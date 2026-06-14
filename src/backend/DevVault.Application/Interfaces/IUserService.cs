using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Users;

namespace DevVault.Application.Interfaces;

public interface IUserService
{
    Task<UserResult> CreateUserAsync (CreateUserDto request);
    Task<PagedResponse<UserResponseDto>> GetUsersAsync (int pageNumber, int pageSize);
}