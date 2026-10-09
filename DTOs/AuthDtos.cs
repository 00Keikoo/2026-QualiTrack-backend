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

public record ChangePasswordRequest(
    [Required][MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
    string NewPassword
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
