namespace DevVault.Domain.Entities;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // The user who performed the action
    public string UserId { get; set; } = string.Empty;
    
    // E.g., "User", "Project", "Secret"
    public string EntityType { get; set; } = string.Empty;
    
    // The ID of the specific item being touched
    public string EntityId { get; set; } = string.Empty;
    
    // E.g., "Create", "Update", "Delete", "Reveal"
    public string Action { get; set; } = string.Empty;
    
    // Any extra context (e.g., "Revealed STRIPE_API_KEY")
    public string Details { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}