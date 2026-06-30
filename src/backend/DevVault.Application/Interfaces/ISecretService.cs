using DevVault.Application.DTOs.Common;
using DevVault.Application.DTOs.Secrets;

namespace DevVault.Application.Interfaces;

public interface ISecretService
{
    // Encrypts and saves a new secret
    Task<Result<SecretResponseDto>> CreateSecretAsync (CreateSecretDto request, string userId);
    // Returns a list of secrets for a project (values remain hidden)
    Task<Result<IEnumerable<SecretResponseDto>>> GetProjectSecretsAsync (string projectId, string userId);
    // Specifically requests to decrypt and view a single secret
    Task<Result<SecretResponseDto>> RevealSecretAsync (string secretId, string projectId, string userId);
    // Permanently deletes a secret
    Task<Result<bool>> DeleteSecretAsync (string secretId, string projectId, string userId);
}
