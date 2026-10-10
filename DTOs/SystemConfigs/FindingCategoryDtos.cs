using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs.SystemConfigs;

public class CreateFindingCategoryDto
{
    [Required(ErrorMessage = "Name kategori wajib diisi")]
    [MaxLength(50, ErrorMessage = "Name Kategori maksimal 50 karakter")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Dekripsi maksimal 200 karakter")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity wajib diisi")]
    [Range(1, 5, ErrorMessage = "Severity harus antara 1-5 (1 = paling parah")]
    public int Severity { get; set; }

    public bool RequiresImmediateAction { get; set; }
}

public class UpdateFindingCategoryDto
{
    [Required(ErrorMessage = "Nama kategori wajib diisi")]
    [MaxLength(50, ErrorMessage = "Nama Kategori maksimal 50 Karakter")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Deskripsi maksimal 200 karakter")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity wajib diisi")]
    [Range(1, 5, ErrorMessage = "Severity harus antara 1 - 5")]
    public int Severity { get; set; }

    public bool RequiresImmediateAction { get; set; }
}

public class FindingCategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool RequiresImmediateAction { get; set; }
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}