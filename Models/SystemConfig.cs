namespace QualiTrack.Models;

public class SystemConfig
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
}