using DevVault.Application.DTOs.Auth;
using DevVault.Application.DTOs.Users;

namespace DevVault.Application.Interfaces;

public interface IUserService
{
    Task<UserResult> CreateUserAsync (CreateUserDto request);
}