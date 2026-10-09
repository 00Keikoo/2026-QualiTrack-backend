using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs;

public class IsoStandardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateIsoStandardRequest
{
    [Required(ErrorMessage = "Nama ISO wajib diisi")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kode ISO wajib diisi")]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateIsoStandardRequest
{
    [Required(ErrorMessage = "Nama ISO wajib diisi")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kode ISO wajib diisi")]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}