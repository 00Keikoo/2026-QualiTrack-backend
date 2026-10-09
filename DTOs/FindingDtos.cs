using System.ComponentModel.DataAnnotations;
using QualiTrack.Models;

namespace QualiTrack.DTOs;

public class CreateFindingRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public Guid? ChecklistItemId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public Guid? ReporterId { get; set; }
    public FindingCategory? Category { get; set; }
    [Required]
    public string Description { get; set; } = string.Empty;
    public string ClauseRef { get; set; } = string.Empty;
}

public class UpdateFindingRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public Guid? ReporterId { get; set; }
    public FindingCategory? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ClauseRef { get; set; } = string.Empty;
}

public class FindingResponseDto
{
    public Guid Id { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? ChecklistItemId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public Guid? ReporterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public FindingCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ClauseRef { get; set; } = string.Empty;
    public DateTime FoundAt { get; set; }
   public FindingStatus Status { get; set; } 
}
