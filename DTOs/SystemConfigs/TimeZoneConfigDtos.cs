using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs.SystemConfigs;

public class UpdateTimeZoneDto
{
    [Required(ErrorMessage = "Timezone wajib diisi")]
    [MaxLength(100, ErrorMessage = "Timezone maksimal 100 karakter")]
    public string TimeZone { get; set; } = "Asia/Jakarta";
}

public class TimeZoneResponseDto
{
    public string TimeZone { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}