using DevVault.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevVault.API.Controllers;

[Authorize(Roles = "Administrator,Auditor")]
[ApiController]
[Route("api/v1/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController (IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var result = await _auditService.GetLogAsync();
        
        if (!result.Success)
        {
            return BadRequest(new { success = false, errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Data });
    }
}
