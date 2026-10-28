using System.Text.Json.Serialization;

namespace QualiTrack.Models;

public class FindingCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HexaCode { get; set; } = "#808080";
    public int Severity { get; set; }
    public bool RequiresImmediateAction { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DeleteAt { get; set; }
    public Guid? DeletedById { get; set; }

    [JsonIgnore]
    public ICollection<Finding> Findings { get; set; } = []; 
}
