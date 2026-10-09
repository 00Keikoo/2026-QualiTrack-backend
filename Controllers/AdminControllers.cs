using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Models;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using System.Security.Claims;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ValidateModelAttribute]
public class AdminController(AppDbContext db) : ControllerBase
{
    private static readonly string[] ValidRoles = UserRoles.AllRoles;

    private static readonly string[] ValidStatuses = UserStatuses.AllStatuses;

    // GET /api/admin/users
    // Daftar semua user dengan filter dan search
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] string? status)
    {
        var query = db.Users.AsNoTracking().AsQueryable();

        // Search by name
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FullName.ToLower().Contains(search.ToLower()));

        // Filter by role
        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role == role);

        // Filter by status
        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.Status == status);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status,
                EmailVerified = u.EmailVerified,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            total = users.Count,
            data = users
        });
    }

    // GET /api/admin/users/{id}
    // Detail satu user
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        return Ok(new UserListDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt
        });
    }

    // PUT /api/admin/users/{id}
    // Update nama dan email user
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest req)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        // Cek email duplikat
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == req.Email && u.Id != id);
        if (emailExists)
            return BadRequest(new { message = "Email sudah dipakai user lain" });

        user.FullName = req.FullName;
        user.Email = req.Email;
        await db.SaveChangesAsync();

        return Ok(new { message = "Data user berhasil diupdate" });
    }

    // PATCH /api/admin/users/{id}/role
    // Update role user
    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest req)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Admin tidak boleh ubah role dirinya sendiri
        if (id == currentUserId)
            return BadRequest(new { message = "Admin tidak dapat mengubah role dirinya sendiri" });

        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        // Admin tidak boleh ubah role Admin lain
        if (user.Role == UserRoles.Admin)
            return BadRequest(new { message = "Admin tidak dapat mengubah role Admin lain" });

        // Validasi role
        if (!ValidRoles.Contains(req.Role))
            return BadRequest(new { message = $"Role tidak valid. Pilih: {string.Join(", ", ValidRoles)}" });

        // Admin tidak boleh assign role Admin ke user lain
        if (req.Role == UserRoles.Admin)
            return BadRequest(new { message = "Tidak dapat mengubah role user menjadi Admin" });

        user.Role = req.Role;
        await db.SaveChangesAsync();

        return Ok(new { message = "Role user berhasil diupdate" });
    }

    // PATCH /api/admin/users/{id}/status
    // Aktifkan/nonaktifkan user
    [HttpPatch("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest req)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Admin tidak boleh nonaktifkan dirinya sendiri
        if (id == currentUserId)
            return BadRequest(new { message = "Admin tidak dapat mengubah status dirinya sendiri" });

        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User tidak ditemukan" });

        // Admin tidak boleh nonaktifkan Admin lain
        if (user.Role == UserRoles.Admin)
            return BadRequest(new { message = "Admin tidak dapat mengubah status Admin lain" });

        if (!ValidStatuses.Contains(req.Status))
            return BadRequest(new { message = "Status tidak valid. Pilih: Active atau Inactive" });

        user.Status = req.Status;
        await db.SaveChangesAsync();

        return Ok(new { message = $"Status user berhasil diubah menjadi {req.Status}" });
    }
}