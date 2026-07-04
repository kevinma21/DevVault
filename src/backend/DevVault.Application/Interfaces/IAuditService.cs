using DevVault.Application.DTOs.Common;

namespace DevVault.Application.Interfaces;

public interface IAuditService
{
    // Fire-and-forget method to record an action
    Task LogActionAsync (string userId, string entityType, string entityId, string action, string details);

    // Fetch logs (for the Admin/Auditor dashboard later)
    Task<Result<IEnumerable<object>>> GetLogAsync();
}
