namespace QualiTrack.Models;

public class FindingCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool RequiresImmediateAction { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Finding> Findings { get; set; } = []; 
}
