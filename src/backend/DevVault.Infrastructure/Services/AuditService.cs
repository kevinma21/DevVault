using DevVault.Application.DTOs.Audit;
using DevVault.Application.DTOs.Common;
using DevVault.Application.Interfaces;
using DevVault.Domain.Entities;
using DevVault.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public async Task<Result<object>> GetLogAsync(int page = 1, int limit = 20)
    {
        try
        {
            var query = _context.AuditLogs
            .Join(
                _context.Users,
                audit => audit.UserId,
                user => user.Id,
                (audit, user) => new AuditLogResponseDto
                {
                    Id = audit.Id,
                    UserEmail = !string.IsNullOrEmpty(user.Email) ? user.Email : (user.FirstName ?? "Unknown User"),
                    EntityType = audit.EntityType,
                    EntityId = audit.EntityId,
                    Action = audit.Action,
                    Details = audit.Details,
                    Timestamp = audit.Timestamp
                }
            );
            
            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            return new Result<object>
            {
                Success = true,
                Data = new
                {
                    items = logs,
                    totalCount = totalCount,
                    currentPage = page,
                    totalPages = totalPages
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs.");
            return new Result<object> { Success = false, Errors = new[] { "Could not retrieve audit logs." } };
        }
    }
}
