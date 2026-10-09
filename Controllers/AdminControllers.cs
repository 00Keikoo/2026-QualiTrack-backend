using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Models;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using QualiTrack.Services;
using System.Security.Claims;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ValidateModelAttribute]
public class AdminController(AppDbContext db, IAdminActivityService activityService) : ControllerBase
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

    // ============================================================
    // ISO STANDARD MANAGEMENT
    // ============================================================

    // GET /api/admin/iso-standards
    [HttpGet("iso-standards")]
    public async Task<IActionResult> GetIsoStandards([FromQuery] bool? isActive)
    {
        var query = db.IsoStandards.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        var data = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IsoStandardDto
            {
                Id = i.Id,
                Name = i.Name,
                Code = i.Code,
                Description = i.Description,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total = data.Count, data });
    }

    // GET /api/admin/iso-standards/{id}
    [HttpGet("iso-standards/{id}")]
    public async Task<IActionResult> GetIsoStandardById(Guid id)
    {
        var iso = await db.IsoStandards
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (iso is null)
            return NotFound(new { message = "ISO Standard tidak ditemukan" });

        return Ok(new IsoStandardDto
        {
            Id = iso.Id,
            Name = iso.Name,
            Code = iso.Code,
            Description = iso.Description,
            IsActive = iso.IsActive,
            CreatedAt = iso.CreatedAt
        });
    }

    // POST /api/admin/iso-standards
    [HttpPost("iso-standards")]
    public async Task<IActionResult> CreateIsoStandard([FromBody] CreateIsoStandardRequest req)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Cek duplikat code
        var codeExists = await db.IsoStandards.AnyAsync(i => i.Code == req.Code);
        if (codeExists)
            return BadRequest(new { message = "Kode ISO sudah digunakan" });

        var iso = new IsoStandard
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Code = req.Code,
            Description = req.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.IsoStandards.Add(iso);
        await db.SaveChangesAsync();

        await activityService.LogAsync(
            adminId,
            AdminActions.Create,
            "IsoStandard",
            iso.Name,
            $"Menambahkan ISO Standard: {iso.Name} ({iso.Code})"
        );

        return CreatedAtAction(nameof(GetIsoStandardById), new { id = iso.Id }, new IsoStandardDto
        {
            Id = iso.Id,
            Name = iso.Name,
            Code = iso.Code,
            Description = iso.Description,
            IsActive = iso.IsActive,
            CreatedAt = iso.CreatedAt
        });
    }

    // PUT /api/admin/iso-standards/{id}
    [HttpPut("iso-standards/{id}")]
    public async Task<IActionResult> UpdateIsoStandard(Guid id, [FromBody] UpdateIsoStandardRequest req)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var iso = await db.IsoStandards.FindAsync(id);
        if (iso is null)
            return NotFound(new { message = "ISO Standard tidak ditemukan" });

        // Cek duplikat code
        var codeExists = await db.IsoStandards.AnyAsync(i => i.Code == req.Code && i.Id != id);
        if (codeExists)
            return BadRequest(new { message = "Kode ISO sudah digunakan" });

        iso.Name = req.Name;
        iso.Code = req.Code;
        iso.Description = req.Description;
        await db.SaveChangesAsync();

        await activityService.LogAsync(
            adminId,
            AdminActions.Update,
            "IsoStandard",
            iso.Name,
            $"Mengubah ISO Standard: {iso.Name} ({iso.Code})"
        );

        return Ok(new { message = "ISO Standard berhasil diupdate" });
    }

    // PATCH /api/admin/iso-standards/{id}/status
    [HttpPatch("iso-standards/{id}/status")]
    public async Task<IActionResult> UpdateIsoStandardStatus(Guid id, [FromBody] bool isActive)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var iso = await db.IsoStandards.FindAsync(id);
        if (iso is null)
            return NotFound(new { message = "ISO Standard tidak ditemukan" });

        iso.IsActive = isActive;
        await db.SaveChangesAsync();

        var action = isActive ? AdminActions.Activate : AdminActions.Deactivate;
        var description = isActive
            ? $"Mengaktifkan ISO Standard: {iso.Name}"
            : $"Menonaktifkan ISO Standard: {iso.Name}";

        await activityService.LogAsync(adminId, action, "IsoStandard", iso.Name, description);

        return Ok(new { message = $"ISO Standard berhasil {(isActive ? "diaktifkan" : "dinonaktifkan")}" });
    }

    // ============================================================
    // SPC UNIT MANAGEMENT
    // ============================================================

    // GET /api/admin/spc-units
    [HttpGet("spc-units")]
    public async Task<IActionResult> GetSpcUnits([FromQuery] bool? isActive)
    {
        var query = db.SpcUnits.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var data = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SpcUnitDto
            {
                Id = s.Id,
                Name = s.Name,
                Symbol = s.Symbol,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total = data.Count, data });
    }

    // GET /api/admin/spc-units/{id}
    [HttpGet("spc-units/{id}")]
    public async Task<IActionResult> GetSpcUnitById(Guid id)
    {
        var unit = await db.SpcUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (unit is null)
            return NotFound(new { message = "SPC Unit tidak ditemukan" });

        return Ok(new SpcUnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Symbol = unit.Symbol,
            Description = unit.Description,
            IsActive = unit.IsActive,
            CreatedAt = unit.CreatedAt
        });
    }

    // POST /api/admin/spc-units
    [HttpPost("spc-units")]
    public async Task<IActionResult> CreateSpcUnit([FromBody] CreateSpcUnitRequest req)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Cek duplikat symbol
        var symbolExists = await db.SpcUnits.AnyAsync(s => s.Symbol == req.Symbol);
        if (symbolExists)
            return BadRequest(new { message = "Symbol unit sudah digunakan" });

        var unit = new SpcUnit
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Symbol = req.Symbol,
            Description = req.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.SpcUnits.Add(unit);
        await db.SaveChangesAsync();

        await activityService.LogAsync(
            adminId,
            AdminActions.Create,
            "SpcUnit",
            unit.Name,
            $"Menambahkan SPC Unit: {unit.Name} ({unit.Symbol})"
        );

        return CreatedAtAction(nameof(GetSpcUnitById), new { id = unit.Id }, new SpcUnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Symbol = unit.Symbol,
            Description = unit.Description,
            IsActive = unit.IsActive,
            CreatedAt = unit.CreatedAt
        });
    }

    // PUT /api/admin/spc-units/{id}
    [HttpPut("spc-units/{id}")]
    public async Task<IActionResult> UpdateSpcUnit(Guid id, [FromBody] UpdateSpcUnitRequest req)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var unit = await db.SpcUnits.FindAsync(id);
        if (unit is null)
            return NotFound(new { message = "SPC Unit tidak ditemukan" });

        // Cek duplikat symbol
        var symbolExists = await db.SpcUnits.AnyAsync(s => s.Symbol == req.Symbol && s.Id != id);
        if (symbolExists)
            return BadRequest(new { message = "Symbol unit sudah digunakan" });

        unit.Name = req.Name;
        unit.Symbol = req.Symbol;
        unit.Description = req.Description;
        await db.SaveChangesAsync();

        await activityService.LogAsync(
            adminId,
            AdminActions.Update,
            "SpcUnit",
            unit.Name,
            $"Mengubah SPC Unit: {unit.Name} ({unit.Symbol})"
        );

        return Ok(new { message = "SPC Unit berhasil diupdate" });
    }

    // PATCH /api/admin/spc-units/{id}/status
    [HttpPatch("spc-units/{id}/status")]
    public async Task<IActionResult> UpdateSpcUnitStatus(Guid id, [FromBody] bool isActive)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var unit = await db.SpcUnits.FindAsync(id);
        if (unit is null)
            return NotFound(new { message = "SPC Unit tidak ditemukan" });

        unit.IsActive = isActive;
        await db.SaveChangesAsync();

        var action = isActive ? AdminActions.Activate : AdminActions.Deactivate;
        var description = isActive
            ? $"Mengaktifkan SPC Unit: {unit.Name}"
            : $"Menonaktifkan SPC Unit: {unit.Name}";

        await activityService.LogAsync(adminId, action, "SpcUnit", unit.Name, description);

        return Ok(new { message = $"SPC Unit berhasil {(isActive ? "diaktifkan" : "dinonaktifkan")}" });
    }
}