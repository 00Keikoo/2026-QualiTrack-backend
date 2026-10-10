using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs.SystemConfigs;

public class UpdateDateFormatDto
{
    [Required(ErrorMessage = "Date format wajib diisi")]
    [MaxLength(20, ErrorMessage = "Date format maksimal 20 karakter")]
    public string DateFormat { get; set; } = "dd-MM-yyyy";
}

public class DateFormatResponseDto
{
    public string DateFormat { get; set; } = string.Empty;
    public DateTime UpdateAt { get; set; }
}