using QualiTrack.Data;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class AdminActivityService(AppDbContext db) : IAdminActivityService
{
    public async Task LogAsync(Guid adminId, string action, string entityType, string? entityName = null, string? description = null)
    {
        db.AdminActivityLogs.Add(new AdminActivityLog
        {
            Id = Guid.NewGuid(),
            AdminId = adminId,
            Action = action,
            EntityType = entityType,
            EntityName = entityName,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}