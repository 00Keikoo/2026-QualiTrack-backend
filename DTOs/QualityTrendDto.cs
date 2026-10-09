namespace QualiTrack.DTOs;

public class QualityTrendDto
{
    public int Period { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public double ComplianceScore { get; set; }
    public double QualityScore { get; set; }
}