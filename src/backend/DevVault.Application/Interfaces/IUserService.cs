using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Users;

namespace DevVault.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponseDto>> CreateUserAsync (CreateUserDto request);
    Task<Result<PagedResponse<UserResponseDto>>> GetUsersAsync (int pageNumber, int pageSize);
    Task<Result<UserResponseDto>> GetUserByIdAsync (string id);
    Task<Result<UserResponseDto>> UpdateUserAsync (string id, UpdateUserDto request);
    Task<Result<bool>> DeactivateUserAsync (string id);
    Task<Result<bool>> AssignSystemRoleAsync (string targetUserId, string role);
}