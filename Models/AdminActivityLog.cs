namespace QualiTrack.Models;

public class AdminActivityLog
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public User? Admin { get; set; }
    public string Action { get; set; } = string.Empty;      // "CREATE", "UPDATE", "DELETE"
    public string EntityType { get; set; } = string.Empty;  // "User", "IsoStandard", dll
    public string? EntityName { get; set; }                 // Nama data yang diubah
    public string? Description { get; set; }                // Detail perubahan
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}