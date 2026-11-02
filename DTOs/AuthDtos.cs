using System.ComponentModel.DataAnnotations;

public record RegisterRequest(
    [Required(ErrorMessage = "Nama Wajib diisi")]
    string FullName,

    [Required(ErrorMessage = "Email Wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    string Email,
    [Required(ErrorMessage = "Password Wajib diisi")]
    string Password,

    [Required(ErrorMessage = "Role Wajib diisi")]
    string Role
);

public record LoginRequest(
    [Required(ErrorMessage = "Email Wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    string Email,
    [Required(ErrorMessage = "Password Wajib diisi")]
    string Password
);

public record AuthResponse(
    string Token,
    string Role,
    string FullName
);

public record UpdateCapaRequest(
    string RootCause,
    string CorrectiveAction,
    string? PreventiveAction,
    DateOnly Deadline,
    Guid? PicId
);

public record ForgotPasswordRequest(
    string Email,
    string NewPassword,
    string ConfirmPassword
);
public record RequestOtpRequest(string Email);

public record VerifyOtpRequest(string Email, string Otp);

public record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword
);

// Catatan: sengaja tanpa [MinLength] deklaratif. Seluruh aturan kekuatan password
// divalidasi terpusat di AuthController.ValidatePasswordStrength supaya pesan error
// konsisten dan sama persis dengan checklist kekuatan password di frontend.
// Sesuai kesepakatan tim, password lama TIDAK diminta pada flow ini.
public record ChangePasswordRequest(
    [Required(ErrorMessage = "Password baru wajib diisi")]
    string NewPassword,

    [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
    string ConfirmPassword
);

public record UpdateProfileRequest(
    [Required] string FullName
);

public record VerifyEmailRequest(string Email, string Otp);
public record ResendOtpRequest(string Email);

public record RequestEmailChangeOtpRequest(
    [Required][EmailAddress(ErrorMessage = "Format email tidak valid")]
    string NewEmail
);

public record VerifyEmailChangeRequest(
    [Required] string Otp
);
