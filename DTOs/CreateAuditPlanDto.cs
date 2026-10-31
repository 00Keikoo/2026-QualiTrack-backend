using System.ComponentModel.DataAnnotations;
using QualiTrack.Models;

namespace QualiTrack.DTOs;

public class CreateAuditPlanDto
{
    [Required(ErrorMessage = "Title wajib diisi")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [Range(2020, 2030)]
    public int Year { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Standard { get; set; } = string.Empty;
    
    public AuditPriority Priority { get; set; } = AuditPriority.Common;

    public string? Description { get; set; }

    public List<CreateScheduleDto>? Schedules { get; set; }
}

public class CreateScheduleDto
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "ClauseRef wajib diisi")]
    public string ClauseRef { get; set; } = string.Empty;

    public string AuditorName {get; set;} = string.Empty;
    
    [Required(ErrorMessage = "ScheduledDate wajib diisi")]
    public DateTime? ScheduledDate { get; set; }
    
    [Required(ErrorMessage = "Department wajib diisi")]
    [StringLength(100, ErrorMessage = "Department maksimal 100 karakter")]
    public string Department { get; set; } = string.Empty;
}

public class NotEmptyGuidAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is Guid guid && guid != Guid.Empty)
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} tidak boleh kosong.");
    }
}