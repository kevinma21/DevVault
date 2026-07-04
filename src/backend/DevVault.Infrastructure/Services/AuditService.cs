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

    public async Task<Result<IEnumerable<object>>> GetLogAsync()
    {
        try
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(100) // limit for performance
                .ToListAsync();

            return new Result<IEnumerable<object>>
            {
                Success = true,
                Data = logs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs.");
            return new Result<IEnumerable<object>> { Success = false, Errors = new[] { "Could not retrieve audit logs." } };
        }
    }
}
