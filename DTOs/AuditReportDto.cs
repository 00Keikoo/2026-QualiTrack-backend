namespace QualiTrack.DTOs;
using QualiTrack.Models;

public class AuditReportListItemDto
{
    public Guid AuditPlanId { get; set; }
    public string AuditPlanTitle { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public AuditPriority Priority { get; set; }
    public Guid ScheduleId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string AuditorName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public Guid SessionId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? SummaryContent { get; set; }
    public int TotalFindings { get; set; }
    public int TotalMajorNC { get; set; }
    public int TotalMinorNC { get; set; }
    public int TotalCAPAs { get; set; }
    public int ClosedCAPAs { get; set; }
}

public class AuditReportDetailDto
{
    public Guid AuditPlanId { get; set; }
    public string AuditPlanTitle { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AuditPriority Priority { get; set; }
    public int Year { get; set; }
    public Guid ScheduleId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string AuditorName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public Guid SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? SummaryContent { get; set; }
    public List<FindingReportDto> Findings { get; set; } = [];
}

public class FindingReportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClauseRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime FoundAt { get; set; }
    public CapaReportDto? Capa { get; set; }
}

public class CapaReportDto
{
    public Guid Id { get; set; }
    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public string? PreventiveAction { get; set; }
    public string PicName { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
