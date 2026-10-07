using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;
using QualiTrack.Services;
using QualiTrack.Filters;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/auth")]
[ValidateModelAttribute]
public class AuthController(AppDbContext db, IConfiguration config, IEmailService emailService) : ControllerBase
{

    private const int OtpExpiryMinutes = 5;
    private const int ResetTokenExpiryMinutes = 10;
    private const int OtpLength = 4;
    private const int OtpMin = 1000;
    private const int OtpMax = 9999;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (!UserRoles.IsValidRole(req.Role))
            return BadRequest(new { message = "Role tidak valid. Pilih: QualityManager, AuditorInternal, Auditee, Admin" });

        if (req.Role == "Admin")
            return BadRequest(new { message = "Role admin tidak dapat didaftarkan secara mandiri. Hubungi adminstrator sistem. " });

        req = req with { Role = UserRoles.NormalizeRole(req.Role) };

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Password minimal 6 karakter" });

        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if(existingUser != null)
        {
            if(existingUser.EmailVerified)
                return BadRequest(new { message = "Email sudah terdaftar"});

            // Daftar ulang sebelum verifikasi: pakai data terbaru dari form (termasuk role)
            existingUser.FullName = req.FullName;
            existingUser.Role = req.Role;
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var newOtp = GenerateSecureOtp();
            existingUser.OtpCode = BCrypt.Net.BCrypt.HashPassword(newOtp);
            existingUser.OtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            await db.SaveChangesAsync();
            await emailService.SendRegistrationOtpAsync(existingUser.Email, newOtp);

            return Ok(new
            {
                message = "Email sudah terdaftar tapi belum diverifikasi. Kode OTP baru telah dikirim ke email kamu.",
                email = existingUser.Email
            });
        }


        var otp = GenerateSecureOtp();

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role,
            Status = "Pending",
            EmailVerified = false,
            OtpCode = BCrypt.Net.BCrypt.HashPassword(otp),
            OtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await emailService.SendRegistrationOtpAsync(user.Email, otp);

        return Ok(new
        {
            message = "Registrasi berhasil! Cek email kamu untuk kode verifikasi.",
            email = user.Email
        });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return NotFound(new { message = "Email tidak ditemukan" });

        if (user.EmailVerified)
            return BadRequest(new { message = "Email sudah diverifikasi" });

        if (user.OtpCode is null || user.OtpExpiry is null)
            return BadRequest(new { message = "OTP belum di-request" });

        if (DateTime.UtcNow > user.OtpExpiry)
            return BadRequest(new { message = "OTP sudah kadaluarsa, minta OTP baru" });

        if (!BCrypt.Net.BCrypt.Verify(req.Otp, user.OtpCode))
            return BadRequest(new { message = "OTP tidak valid" });

        user.EmailVerified = true;
        user.Status = "Active";
        user.OtpCode = null;
        user.OtpExpiry = null;
        await db.SaveChangesAsync();

        return Ok(new { message = "Email berhasil diverifikasi! Silakan login." });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return NotFound(new { message = "Email tidak ditemukan" });

        if (user.EmailVerified)
            return BadRequest(new { message = "Email sudah diverifikasi" });

        var otp = GenerateSecureOtp();
        user.OtpCode = BCrypt.Net.BCrypt.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        await db.SaveChangesAsync();

        await emailService.SendRegistrationOtpAsync(user.Email, otp);

        return Ok(new { message = "Kode OTP baru telah dikirim ke email kamu" });

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email atau password salah" });

        // TODO akhir Sprint 2: aktifkan kembali setelah email service siap
        if (!user.EmailVerified)
            return Unauthorized(new { message = "Email belum diverifikasi. Cek inbox email kamu." });

        var token = GenerateJwt(user);
        return Ok(new AuthResponse(token, UserRoles.GetClaimRole(user.Role), user.FullName));
    }

    [HttpGet("whoami")]
    [Authorize]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            isAuthenticated = User.Identity != null && User.Identity.IsAuthenticated,
            name = User.Identity != null ? User.Identity.Name : null,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logout berhasil" });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound(new { message = "User tidak ditemukan" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync();

        return Ok(new { message = "Password berhasil diubah" });
    }

    [HttpPost("forgot-password/request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return NotFound(new { message = "Email tidak ditemukan" });

        var otp = GenerateSecureOtp();
        user.OtpCode = BCrypt.Net.BCrypt.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        await db.SaveChangesAsync();

        await emailService.SendOtpAsync(user.Email, otp);

        return Ok(new { message = "Kode OTP telah dikirim ke email kamu" });
    }

    [HttpPost("forgot-password/verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return NotFound(new { message = "Email tidak ditemukan" });

        if (user.OtpCode is null || user.OtpExpiry is null)
            return BadRequest(new { message = "OTP belum di-request" });

        if (DateTime.UtcNow > user.OtpExpiry)
            return BadRequest(new { message = "OTP sudah kadaluarsa, minta OTP baru" });

        if (!BCrypt.Net.BCrypt.Verify(req.Otp, user.OtpCode))
            return BadRequest(new { message = "OTP tidak valid" });

        var resetToken = Guid.NewGuid().ToString();
        user.OtpCode = resetToken;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes);
        await db.SaveChangesAsync();

        return Ok(new { message = "OTP valid", resetToken = resetToken });
    }

    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return NotFound(new { message = "Email tidak ditemukan" });

        if (user.OtpCode is null || user.OtpExpiry is null)
            return BadRequest(new { message = "Reset token tidak valid" });

        if (DateTime.UtcNow > user.OtpExpiry)
            return BadRequest(new { message = "Reset token sudah kadaluarsa" });

        if (user.OtpCode != req.ResetToken)
            return BadRequest(new { message = "Reset token tidak valid" });

        if (req.NewPassword.Length < 6)
            return BadRequest(new { message = "Password minimal 6 karakter" });

        if (string.Equals(req.NewPassword, req.ConfirmPassword) == false)
            return BadRequest(new { message = "Password dan konfirmasi password tidak sama" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.OtpCode = null;
        user.OtpExpiry = null;
        await db.SaveChangesAsync();

        return Ok(new { message = "Password berhasil direset, silakan login" });
    }

    [HttpPost("request-email-change-otp")]
    [Authorize]
    public async Task<IActionResult> RequestEmailChangeOtp([FromBody] RequestEmailChangeOtpRequest req)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound(new { message = "User tidak ditemukan" });

        var emailTaken = await db.Users.AnyAsync(u => u.Email == req.NewEmail && u.Id != userId);
        if (emailTaken) return BadRequest(new { message = "Email sudah digunakan oleh akun lain" });

        var otp = GenerateSecureOtp();
        user.PendingEmail = req.NewEmail;
        user.OtpCode = BCrypt.Net.BCrypt.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        await db.SaveChangesAsync();

        await emailService.SendOtpAsync(req.NewEmail, otp);

        return Ok(new { message = $"OTP telah dikirim ke {req.NewEmail}. Berlaku 5 menit." });
    }

    [HttpPost("verify-email-change")]
    [Authorize]
    public async Task<IActionResult> VerifyEmailChange([FromBody] VerifyEmailChangeRequest req)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound(new { message = "User tidak ditemukan" });

        if (string.IsNullOrEmpty(user.PendingEmail))
            return BadRequest(new { message = "Tidak ada permintaan ganti email yang aktif" });

        if (user.OtpCode is null || user.OtpExpiry is null)
            return BadRequest(new { message = "OTP belum di-request" });

        if (DateTime.UtcNow > user.OtpExpiry)
            return BadRequest(new { message = "OTP sudah kadaluarsa, minta OTP baru" });

        if (!BCrypt.Net.BCrypt.Verify(req.Otp, user.OtpCode))
            return BadRequest(new { message = "OTP tidak valid" });

        user.Email = user.PendingEmail;
        user.PendingEmail = null;
        user.OtpCode = null;
        user.OtpExpiry = null;
        await db.SaveChangesAsync();

        return Ok(new { message = "Email berhasil diperbarui", newEmail = user.Email });
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, UserRoles.GetClaimRole(user.Role)),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureOtp()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[OtpLength];
        rng.GetBytes(bytes);
        var value = BitConverter.ToUInt32(bytes, 0);
        return (value % 9000 + 1000).ToString();
    }
}
