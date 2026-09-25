using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs;

public class UpdateChecklistDto
{
    [Required(ErrorMessage = "Judul checklist wajib diisi")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Standard wajib diisi")]
    public string Standard { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department wajib diisi")]
    public string Department { get; set; } = string.Empty;

    // Urutan di list = urutan item (OrderIndex)
    public List<UpdateChecklistItemDto> Items { get; set; } = [];
}

public class UpdateChecklistItemDto
{
    // null = item baru
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Pertanyaan checklist wajib diisi")]
    public string Question { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string ClauseRef { get; set; } = string.Empty;
}
