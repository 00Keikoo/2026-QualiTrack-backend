using System.ComponentModel.DataAnnotations;
using QualiTrack.Models;

namespace QualiTrack.DTOs;

public class CreateCapaRequest
{
    [Required(ErrorMessage = "RootCause wajib diisi")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "RootCause harus antara 5 sampai 1000 karakter")]
    public string RootCause { get; set; } = string.Empty;

    [Required(ErrorMessage = "CorrectiveAction wajib diisi")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "CorrectiveAction harus antara 5 sampai 1000 karakter")]
    public string CorrectiveAction { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "PreventiveAction maksimal 1000 karakter")]
    public string? PreventiveAction { get; set; }

    [Required(ErrorMessage = "Deadline wajib diisi")]
    public DateOnly? Deadline { get; set; }
    public Guid? PicId { get; set; }
    public string? PicName { get; set; }
}

public class UpdateCapaRequestDto
{
    [Required(ErrorMessage = "RootCause wajib diisi")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "RootCause harus antara 5 sampai 1000 karakter")]
    public string RootCause { get; set; } = string.Empty;

    [Required(ErrorMessage = "CorrectiveAction wajib diisi")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "CorrectiveAction harus antara 5 sampai 1000 karakter")]
    public string CorrectiveAction { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "PreventiveAction maksimal 1000 karakter")]
    public string? PreventiveAction { get; set; }

    [Required(ErrorMessage = "Deadline wajib diisi")]
    public DateOnly? Deadline { get; set; }

    public Guid? PicId { get; set; }
    public string? PicName { get; set; }
}

public class AddCapaActionRequest
{
    [Required(ErrorMessage = "Description wajib diisi")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Description harus antara 5 sampai 1000 karakter")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "DoneById wajib diisi")]
    public Guid? DoneById { get; set; }
}

public class CloseOutVerificationRequest
{
    [Required(ErrorMessage = "IsEffective wajib diisi")]
    public bool? IsEffective { get; set; }

    [Required(ErrorMessage = "VerificationNotes wajib diisi")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "VerificationNotes harus antara 5 sampai 2000 karakter")]
    public string VerificationNotes { get; set; } = string.Empty;

    [Required(ErrorMessage = "VerifiedById wajib diisi")]
    public Guid? VerifiedById { get; set; }
}


public class CAPAResponseDto
{
    public Guid Id { get; set; }
    public Guid FindingId { get; set; }
    public string FindingTitle { get; set; } = string.Empty;
    public string FindingCategory { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public string? PreventiveAction { get; set; }
    public DateOnly Deadline { get; set; }
    public CAPAStatus Status { get; set; }
    public Guid? PicId { get; set; }
    public string? PicName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CAPAActionResponseDto> Actions { get; set; } = new();
    public CloseOutResponseDto? CloseOut { get; set; }
}

// Response DTO untuk CAPA Action
public class CAPAActionResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DoneAt { get; set; }
    public Guid? DoneById { get; set; }
    public string? DoneByName { get; set; }
    // JANGAN tambahkan properti Capa atau CapaId di sini!
    // Atau jika perlu CapaId, hanya ID-nya saja:
    public Guid? CapaId { get; set; }
}

// Response DTO untuk Close Out Verification
public class CloseOutResponseDto
{
    public Guid Id { get; set; }
    public bool IsEffective { get; set; }
    public string VerificationNotes { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public Guid? VerifiedById { get; set; }
    public string? VerifiedByName { get; set; }
    // JANGAN tambahkan properti Capa di sini!
    public Guid? CapaId { get; set; } // Hanya ID jika perlu
}
