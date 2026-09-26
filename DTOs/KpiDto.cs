namespace QualiTrack.DTOs;

public class KpiDto
{
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalCompletedOnTime { get; set; }
    public int TotalOverdue { get; set; }
    public int TotalStalled { get; set; }
    public double OnTimeRate { get; set; }
    public double ComplianceScore { get; set; }
    public double QualityScore { get; set; }
    public int TotalFindingsReported { get; set; }
}
