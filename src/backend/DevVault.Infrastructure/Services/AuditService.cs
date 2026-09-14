using DevVault.Application.DTOs.Audit;
using DevVault.Application.DTOs.Common;
using DevVault.Application.Interfaces;
using DevVault.Domain.Entities;
using DevVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevVault.Infrastructure.Service;

public class AuditService : IAuditService
{
    private readonly DevVaultDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService (DevVaultDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActionAsync(string userId, string entityType, string entityId, string action, string details)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // We log this to the console, but we don't want a logging failure 
            // to crash the main application workflow.
            _logger.LogError(ex, "Failed to write audit log for User {UserId}, Action {Action}", userId, action);
        }
    }

    public async Task<Result<IEnumerable<AuditLogResponseDto>>> GetLogAsync()
    {
        try
        {
            var logs = await _context.AuditLogs
            .Join(
                _context.Users,
                audit => audit.UserId,
                user => user.Id,
                (audit, user) => new AuditLogResponseDto
                {
                    Id = audit.Id,
                    UserEmail = !string.IsNullOrEmpty(user.Email) ? user.Email : (user.UserName ?? "Unknown User"),
                    EntityType = audit.EntityType,
                    EntityId = audit.EntityId,
                    Action = audit.Action,
                    Details = audit.Details,
                    Timestamp = audit.Timestamp
                }
            )
            .OrderByDescending(a => a.Timestamp)
            .Take(100) 
            .ToListAsync();

            return new Result<IEnumerable<AuditLogResponseDto>> { Success = true, Data = logs };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs.");
            return new Result<IEnumerable<AuditLogResponseDto>> { Success = false, Errors = new[] { "Could not retrieve audit logs." } };
        }
    }
}
