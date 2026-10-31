using QualiTrack.Models;
namespace QualiTrack.Models;

public class EvidenceFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty; // Supabase Storage path
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    // Foreign Keys
    public Guid? FindingId { get; set; } 
    public Guid? AuditResponseId { get; set; }
    public Guid? CapaActionId { get; set; }
    // Navigation Properties
    public Finding? Finding { get; set; }
    public AuditResponse? AuditResponse { get; set; }
    public CAPAAction? CapaAction { get; set; }
}