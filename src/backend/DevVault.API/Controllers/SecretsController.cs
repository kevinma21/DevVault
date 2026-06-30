using System.Security.Claims;
using DevVault.Application.DTOs.Secrets;
using DevVault.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevVault.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/projects/{projectId}/[controller]")]
public class SecretsController : ControllerBase
{
    private readonly ISecretService _secretService;

    public SecretsController (ISecretService secretService)
    {
        _secretService = secretService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSecret (string projectId, [FromBody] CreateSecretDto request)
    {
        // Ensure the DTO project ID matches the URL
        request.ProjectId = projectId;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _secretService.CreateSecretAsync(request, userId);
        if (!result.Success) return BadRequest(new { success = false, message = "Failed to create secret.", errors = result.Errors });

        return Ok(new { success = true, message = "Secret securely encrypted and stored.", data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetSecrets (string projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _secretService.GetProjectSecretsAsync(projectId, userId);
        if (!result.Success) return BadRequest(new { success = false, errors = result.Errors });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("{secretId}/reveal")]
    public async Task<IActionResult> RevealSecret (string projectId, string secretId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _secretService.RevealSecretAsync(secretId, projectId, userId);
        if (!result.Success) return BadRequest(new { success = false, errors = result.Errors });

        // WARNING: This response contains the plain-text API key
        return Ok(new { success = true, data = result.Data });
    }

    [HttpDelete("{secretId}")]
    public async Task<IActionResult> DeleteSecret(string projectId, string secretId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _secretService.DeleteSecretAsync(secretId, projectId, userId);
        if (!result.Success) return BadRequest(new { success = false, errors = result.Errors });

        return Ok(new { success = true, message = "Secret permanently deleted." });
    }
}