// Models/AuditSummary.cs
namespace QualiTrack.Models;

public class AuditSummary
{
    public Guid Id { get; set; }
    public Guid AuditSessionId { get; set; }
    public AuditSession AuditSession { get; set; } = null!;
    public string Content { get; set; } = string.Empty; // ← isi teks summary dari user
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}