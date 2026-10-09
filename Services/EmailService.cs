using MimeKit;
using System.Net.Http.Json;
using MailKit.Net.Smtp;   // Uncomment jika mau balik ke SMTP Gmail
using MailKit.Security;   // Uncomment jika mau balik ke SMTP Gmail

namespace QualiTrack.Services;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otp);
    Task SendRegistrationOtpAsync(string toEmail, string otp);
}

public class EmailService(IConfiguration config) : IEmailService
{
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        // [A] SMTP Gmail — comment blok B, uncomment ini:
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(config["Email:From"]!));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlBody };
        using var smtp = new SmtpClient();
        smtp.ServerCertificateValidationCallback = (_, _, _, _) => true;
        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(config["Email:From"]!, config["Email:Password"]!);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        await SendEmailAsync(toEmail,
            "QualiTrack - Kode OTP Reset Password",
            BuildOtpHtml("Reset Password",
                "Kamu menerima email ini karena ada permintaan reset password.",
                otp,
                "Jika kamu tidak meminta reset password, abaikan email ini."));
    }

    public async Task SendRegistrationOtpAsync(string toEmail, string otp)
    {
        await SendEmailAsync(toEmail,
            "QualiTrack - Verifikasi Email",
            BuildOtpHtml("Verifikasi Email",
                "Selamat datang di QualiTrack! 🎉 Gunakan kode di bawah untuk verifikasi email kamu.",
                otp,
                "Jika kamu tidak mendaftar di QualiTrack, abaikan email ini."));
    }

    private static string BuildOtpHtml(string heading, string intro, string otp, string footer) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; color: #333;">
            <h2 style="color: #1a3a5c;">QualiTrack — {heading}</h2>
            <p>{intro}</p>
            <p>Gunakan kode OTP berikut:</p>
            <div style="background: #f0f4f8; padding: 20px; text-align: center; border-radius: 8px; margin: 16px 0;">
                <h1 style="color: #1a3a5c; letter-spacing: 8px; margin: 0;">{otp}</h1>
            </div>
            <p>Kode berlaku selama <strong>5 menit</strong>.</p>
            <p>{footer}</p>
            <br>
            <p style="color: #888; font-size: 13px;">QualiTrack Team</p>
        </div>
        """;
}
