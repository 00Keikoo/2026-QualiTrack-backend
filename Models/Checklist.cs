using QualiTrack.Models;
namespace QualiTrack.Models;

public class Checklist
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? IsoStandardId { get; set; }
    public IsoStandard? IsoStandard { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? DepartmentNavigation { get; set; }
    // Protection Flag
    public bool IsSystemTemplate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChecklistItem> Items { get; set; } = [];

}