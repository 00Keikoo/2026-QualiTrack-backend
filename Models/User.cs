using System.ComponentModel.DataAnnotations;
using QualiTrack.Constants;

namespace QualiTrack.Models;

public class User
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password wajib diisi")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role wajib diisi")]
    public string Role { get; set; } = "QualityManager";
    
    public string Status { get; set; } = UserStatus.Active;

    public bool EmailVerified { get; set; } = false;
	
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public string? ProfilePhotoUrl { get; set; }    
    public string? PendingEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
