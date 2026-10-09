namespace QualiTrack.Services;

public interface IAdminActivityService
{
    Task LogAsync(Guid adminId, string action, string entityType, string? entityName = null, string? description = null);
}