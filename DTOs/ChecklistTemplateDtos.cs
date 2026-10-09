using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QualiTrack.DTOs;

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ClauseRef { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

// Response DTO untuk GET list
public class ChecklistTemplateListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? IsoStandardId { get; set; }
    public string IsoStandardName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsSystemTemplate { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// GET Detail (dengan items)
public class ChecklistTemplateDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? IsoStandardId { get; set; }
    public string IsoStandardName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsSystemTemplate { get; set; }
    public List<ChecklistItemDto> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

// (POST)
public class CreateChecklistTemplateDto
{
    [Required(ErrorMessage = "Judul template wajib diisi")]
    [MaxLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
    public string Title { get; set; } = string.Empty;

    public Guid? IsoStandardId { get; set; }

    [Required(ErrorMessage = "Department wajib dipilih")]
    public Guid DepartmentId { get; set; }

    // Item bisa kosong saat pertama create
    public List<CreateChecklistItemDto> Items { get; set; } = [];
}

public class CreateChecklistItemDto
{
    [Required(ErrorMessage = "Pertanyaan wajib diisi")]
    [MaxLength(500, ErrorMessage = "Pertanyaan maksimal 500 karakter")]
    public string Question { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Desktripsi maksimal 1000 karakter")]
    public string? Description { get; set; }

    [Required(ErrorMessage = " ClauseRef wajib diisi")]
    [MaxLength(50, ErrorMessage = "ClauseRef maksimal 50 karakter")]
    public string ClauseRef { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "OrderIndex harus lebih dari 0")]
    public int OrderIndex { get; set; }
}