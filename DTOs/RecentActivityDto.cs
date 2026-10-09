namespace QualiTrack.DTOs;

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    // CapaAction | CapaVerified | FindingReported
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid RelatedId { get; set; } // Id Capa/Finding terkait (buat fe)

}