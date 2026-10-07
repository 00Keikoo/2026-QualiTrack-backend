using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IKpiService _kpiService;
    private readonly IRecentActivityService _activityService;
    private readonly AppDbContext _db;

    public ProfileController(
        IKpiService kpiService,
        IRecentActivityService activityService,
        AppDbContext db)
    {
        _kpiService = kpiService;
        _activityService = activityService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status,
            user.ProfilePhotoUrl
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound(new
        {
            message = "User tidak ditemukan"
        });

        user.FullName = req.FullName;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Profil berhasil diupdate",
            data = new
            {
                user.FullName,
                user.Email
            }
        });
    }

    [HttpGet("kpi")]
    public async Task<IActionResult> GetMyKpi()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var kpi = await _kpiService.GetUserKpiAsync(userId);
        return Ok(kpi);
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetMyRecentActivity([FromQuery] int limit = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var activity = await _activityService.GetUserRecentActivityAsync(userId, limit);
        return Ok(activity);
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new
            {
                message = "file harus berupa gambar (jpg / png)"
            });

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
        {
            var oldPath = Path.Combine("uploads", "profiles", Path.GetFileName(user.ProfilePhotoUrl));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var uploadDir = Path.Combine("uploads", "profiles");
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        user.ProfilePhotoUrl = $"/uploads/profiles/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Foto profil berhasil di upload", url = user.ProfilePhotoUrl });
    }

    [HttpDelete("photo")]
    public async Task<IActionResult> RemoveProfilePhoto()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        if (string.IsNullOrEmpty(user.ProfilePhotoUrl))
            return BadRequest(new { message = "Tidak ada foto profil yang bisa dihapus" });

        var filePath = Path.Combine("uploads", "profiles", Path.GetFileName(user.ProfilePhotoUrl));
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "File foto profil tidak ditemukan " });

        System.IO.File.Delete(filePath);

        user.ProfilePhotoUrl = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Foto profil berhasil dihapus" });
    }
}
