using QualiTrack.Models;

namespace QualiTrack.Models;

public class CAPA
{
    public Guid Id { get; set; }
    public Guid FindingId { get; set; }
    public Finding? Finding { get; set; }
    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public string? PreventiveAction { get; set; }
    public Guid PicId { get; set; }
    public User? Pic { get; set; }
    public DateOnly Deadline { get; set; }
    public CAPAStatus Status { get; set; } = CAPAStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public CloseOutVerification? CloseOut { get; set; }
    public ICollection<CAPAAction> Actions { get; set; } = [];
}

public enum CAPAStatus { Open, InProgress, PendingVerification, Closed }
