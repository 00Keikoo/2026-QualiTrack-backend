using System.ComponentModel.DataAnnotations;
using QualiTrack.Models;

namespace QualiTrack.DTOs;

public class UpdateAuditPlanDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [Range(2020, 2030)]
    public int Year { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Standard { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AuditPriority Priority { get; set; } = AuditPriority.Common;
    
    public List<CreateScheduleDto>? Schedules { get; set; }
}