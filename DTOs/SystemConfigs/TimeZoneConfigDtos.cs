using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs.SystemConfigs;

public class UpdateTimeZoneDto
{
    [Required(ErrorMessage = "Timezone wajib diisi")]
    public string TimeZone { get; set; } = "WIB";
}

public class TimeZoneResponseDto
{
    public string TimeZone { get; set; } = string.Empty;
    public string GetSystemTimeZoneId { get; set; } = string.Empty;
    public int UtcOffsets { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TimeZoneOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SystemTimeZoneId { get; set; } = string.Empty;
    public int UtcOffsets { get; set; }
}