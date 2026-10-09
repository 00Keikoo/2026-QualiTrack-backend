using QualiTrack.Models;

namespace QualiTrack.Models;

public class AuditSession
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public AuditSchedule Schedule { get; set; } = null!;
    public Guid ChecklistId { get; set; }
    public Checklist Checklist { get; set; } = null!;
    public AuditSessionStatus Status { get; set; } = AuditSessionStatus.InProgress;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set;} = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<AuditResponse> Responses { get; set; } = [];
    public ICollection<Finding> Findings { get; set; } = [];
    public AuditSummary? Summary { get; set; }
}

public enum AuditSessionStatus { InProgress, Completed, Cancelled }
