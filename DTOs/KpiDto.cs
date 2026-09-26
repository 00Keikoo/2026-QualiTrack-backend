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
    public double OnTimeCompletionRate { get; set; } // 0.0 - 1.0

    // KPI tugas: Auditee dihitung dari CAPA, role lain dari jadwal audit
    public string KpiBasis { get; set; } = "Audit"; // "Audit" | "Capa"
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalCompletedOnTime { get; set; }
    public int TotalOverdue { get; set; }
    public double QualityScore { get; set; } // selesai tepat waktu / yang sudah dikerjakan (0.0 - 1.0)
    public double SuccessRate { get; set; }  // selesai / seluruh tugas yang diberikan (0.0 - 1.0)
}
