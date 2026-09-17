using QualiTrack.Models;
namespace QualiTrack.Models;

public class CloseOutVerification
{
    public Guid Id { get; set; }
    public Guid CapaId { get; set; }
    public CAPA? Capa { get; set; }
    public bool IsEffective { get; set; }
    public string VerificationNotes { get; set; } = string.Empty;
    public Guid VerifiedById { get; set; }
    public User? VerifiedBy { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
}