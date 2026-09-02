namespace QualiTrack.DTOs;

public class DepartmentTrendDto
{
    public string Department { get; set; } = string.Empty;
    public List<PeriodScoreDto> Data { get; set; } = [];
}

public class PeriodScoreDto
{
    public string PeriodLabel { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public double ComplianceScore { get; set; }
    public int TotalSessions { get; set; }
}