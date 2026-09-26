using QualiTrack.Models;

namespace QualiTrack.Models;

public class ChecklistItem
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
    public Checklist? Checklist { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ClauseRef { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
