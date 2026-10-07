using System.ComponentModel.DataAnnotations;

namespace QualiTrack.DTOs;

public class UserListDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateUserRoleRequest
{
    [Required(ErrorMessage = "Role wajib diisi")]
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserStatusRequest
{
    [Required(ErrorMessage = "Status wajib diisi")]
    public string Status { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Nama wajib diisi")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;
}