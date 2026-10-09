using QualiTrack.Models;

namespace QualiTrack.Models;

public class CAPAAction
{
    public Guid Id { get; set; }
    public Guid CapaId { get; set; }
    public CAPA? Capa { get; set; } 
    public string Description { get; set; } = string.Empty;
    public Guid DoneById { get; set; }
    public User? DoneBy { get; set; }
    public DateTime DoneAt { get; set; } = DateTime.UtcNow;
    public ICollection<EvidenceFile> Evidences { get; set; } = [];
}