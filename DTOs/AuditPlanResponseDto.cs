using System.ComponentModel.DataAnnotations;  
namespace QualiTrack.DTOs;

using QualiTrack.Models;

public class AuditPlanResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Standard { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public AuditPriority Priority { get; set; }
    public int TotalSchedules { get; set; }
    public List<ScheduleResponseDto> Schedules { get; set; } = new();
}

public class ScheduleResponseDto
{
    public Guid Id { get; set; }
    public string ClauseRef { get; set; } = string.Empty;
    public Guid? AuditorId { get; set; }
    public string AuditorName { get; set; } = string.Empty;  // Join dari User
    public DateTime ScheduledDate { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public bool IsFinished { get; set; }
}

public class BatchAuditResponseDto
{
    public Guid SessionId { get; set; }
    public List<AuditResponseItemDto> Responses { get; set; } = [];
}

public class AuditResponseItemDto
{
    public Guid ChecklistItemId { get; set; }
    public bool IsPassed { get; set; }
    public string? Notes { get; set; }
}

public class SaveProgressDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid ChecklistItemId { get; set; }
    public bool IsPassed { get; set; }

    public string? Notes { get; set; }
}