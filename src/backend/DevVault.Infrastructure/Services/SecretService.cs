using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Secrets;
using DevVault.Application.Interfaces;
using DevVault.Domain.Entities;
using DevVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DevVault.Infrastructure.Services;

public class SecretService : ISecretService
{
    private readonly DevVaultDbContext _context;
    private readonly ICryptographyService _cryptoService;
    private readonly ILogger<SecretService> _logger;

    public SecretService(DevVaultDbContext context, ICryptographyService cryptoService, ILogger<SecretService> logger)
    {
        _context = context;
        _cryptoService = cryptoService;
        _logger = logger;
    }

    // Helper method to verify Tenant Isolation
    private async Task<bool> UserHasProjectAccessAsync (string projectId, string userId)
    {
        var isOwner = await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        var isMember = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        return isOwner || isMember;
    }

    public async Task<Result<SecretResponseDto>> CreateSecretAsync (CreateSecretDto request, string userId)
    {
        try
        {
            if (!await UserHasProjectAccessAsync(request.ProjectId, userId))
            {
                return new Result<SecretResponseDto>
                {
                    Success = false,
                    Errors = new[] { "Access denied to this project." }
                };
            }

            // ENCRYPTION 
            var encryptedValue = _cryptoService.Encrypt(request.Value);

            var secret = new Secret
            {
                ProjectId = request.ProjectId,
                Key = request.Key,
                EncryptedValue = encryptedValue,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Secrets.AddAsync(secret);
            await _context.SaveChangesAsync();

            var result = new SecretResponseDto
            {
                Id = secret.Id,
                ProjectId = secret.ProjectId,
                Key = secret.Key,
                CreatedAt = secret.CreatedAt
            };

            return new Result<SecretResponseDto>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret for project {ProjectId}", request.ProjectId);
            return new Result<SecretResponseDto>
            {
                Success = false,
                Errors = new[] { "An error occurred while saving the secret." }
            };
        }
    }
    
    public async Task<Result<IEnumerable<SecretResponseDto>>> GetProjectSecretsAsync (string projectId, string userId)
    {
        try
        {
            if (!await UserHasProjectAccessAsync(projectId, userId))
            {
                return new Result<IEnumerable<SecretResponseDto>>
                {
                    Success = false,
                    Errors = new[]
                    {
                        "Access denied."
                    }
                };
            }

            var secrets = await _context.Secrets
                .Where(s => s.ProjectId == projectId)
                .Select(s => new SecretResponseDto
                {
                    Id = s.Id,
                    ProjectId = s.ProjectId,
                    Key = s.Key,
                    // Values remain hidden when just listing secrets
                })
                .ToListAsync();

            return new Result<IEnumerable<SecretResponseDto>>
            {
                Success = true,
                Data = secrets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching secrets for project {projectId}", projectId);
            return new Result<IEnumerable<SecretResponseDto>> 
            { 
                Success = false, 
                Errors = new[] { "An unexpected error occurred." } 
            };
        }
    }

    public async Task<Result<SecretResponseDto>> RevealSecretAsync (string secretId, string projectId, string userId)
    {
        try
        {
            if (!await UserHasProjectAccessAsync(projectId, userId))
            {
                return new Result<SecretResponseDto>
                {
                    Success = false,
                    Errors = new[] { "Access denied." }
                };
            }

            var secret = await _context.Secrets.FirstOrDefaultAsync(s => s.Id == secretId && s.ProjectId == projectId );
            if (secret == null) return new Result<SecretResponseDto> { Success = false, Errors = new[] { "Secret not found." }};

            var plainTextValue = _cryptoService.Decrypt(secret.EncryptedValue);

            var result = new SecretResponseDto
            {
                Id = secret.Id,
                ProjectId = secret.ProjectId,
                Key = secret.Key,
                DecryptedValue = plainTextValue,
                CreatedAt = secret.CreatedAt
            };

            return new Result<SecretResponseDto>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revealing secret {secretId}", secretId);
            return new Result<SecretResponseDto>
            {
                Success = false,
                Errors = new[] { "An error occurred while decrypting the secret." }
            };
        }
    }

    public async Task<Result<bool>> DeleteSecretAsync (string secretId, string projectId, string userId)
    {
        try
        {
            if (!await UserHasProjectAccessAsync(projectId, userId))
            {
                return new Result<bool>
                {
                    Success = false,
                    Errors = new[] { "Access denied." }
                };
            }

            var secret = await _context.Secrets.FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Id == secretId);
            if (secret == null) return new Result<bool> { Success = false, Errors = new[] { "Secret not found." }};

            _context.Secrets.Remove(secret);
            await _context.SaveChangesAsync();

            return new Result<bool>
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret {secretId}", secretId);
            return new Result<bool> { Success = false, Errors = new[] { "An unexpected error occurred." } };
        }
    }
}
