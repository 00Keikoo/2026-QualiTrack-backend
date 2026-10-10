using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs.SystemConfigs;

public class CreateCapaStatusDto
{
    [Required(ErrorMessage = "Nama status wajib diisi")]
    [MaxLength(50, ErrorMessage = "Nama status maksimal 50 karakter")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Deskripsi maksimal 200 karakter")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Order index wajib diisi")]
    [Range(1, 100, ErrorMessage = "Order index harus natara 1-100")]
    public int OrderIndex { get; set; }
    public bool IsTerminal { get; set; }
}

public class UpdateCapaStatusDto
{
    [Required(ErrorMessage = "Nama status wajib diisi")]
    [MaxLength(50, ErrorMessage = "Nama status maksimal 50 karakter")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Deskripsi maksimal 200 karakter")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Order index wajib diisi")]
    [Range(1, 100, ErrorMessage = "Order index harus antara 1-100")]
    public int OrderIndex { get; set; }
    public bool IsTerminal { get; set; }
}

public class CapaStatusResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsTerminal { get; set; }
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
