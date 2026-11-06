namespace QualiTrack.Models;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    // Path/key file logo yang disimpan di storage (uploads/xxx.png). Null = belum ada logo.
    public string? LogoUrl { get; set; }
    // Warna aksen department dalam format HEX (#RRGGBB). Kosong = ditentukan otomatis oleh program.
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } 

    public ICollection<Checklist> Checklists { get; set; } = [];
}