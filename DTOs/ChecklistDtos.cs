using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs;

public class UpdateChecklistDto
{
    [Required(ErrorMessage = "Judul checklist wajib diisi")]
    public string Title { get; set; } = string.Empty;

    public Guid? IsoStandardId { get; set; }
    public Guid? DepartmentId { get; set; }

    [Required(ErrorMessage = "Standard wajib diisi")]
    public string Standard { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department wajib diisi")]
    public string Department { get; set; } = string.Empty;

    // Urutan di list = urutan item (OrderIndex)
    [MinLength(1, ErrorMessage = "Template harus memiliki minimal 1 item")]
    public List<UpdateChecklistItemDto> Items { get; set; } = [];
}

public class UpdateChecklistItemDto
{
    // null = item baru
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Pertanyaan checklist wajib diisi")]
    [MaxLength(500, ErrorMessage = "Pertanyaan maksimal 500 karakter")]
    public string Question { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Clause reference wajib diisi")]
    public string ClauseRef { get; set; } = string.Empty;
}
