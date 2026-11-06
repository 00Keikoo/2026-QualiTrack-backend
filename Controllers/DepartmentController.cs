using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController(AppDbContext db, IStorageService storage, FileValidationService fileValidation) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IStorageService _storage = storage;
    private readonly FileValidationService _fileValidation = fileValidation;

    // Palet warna yang dipakai program saat admin tidak memilih warna sendiri.
    private static readonly string[] AutoColorPalette =
    [
        "#2563EB", "#9333EA", "#DB2777", "#059669",
        "#0284C7", "#EA580C", "#0891B2", "#7C3AED"
    ];

    private static readonly Regex HexColorRegex = new("^#([0-9A-Fa-f]{6})$", RegexOptions.Compiled);

    // Normalisasi input warna ("2563EB" / "#2563eb") -> "#2563EB". Null kalau format tidak valid.
    private static string? NormalizeColor(string? color)
    {
        var raw = color?.Trim();
        if (string.IsNullOrEmpty(raw)) return null;
        if (!raw.StartsWith('#')) raw = $"#{raw}";
        return HexColorRegex.IsMatch(raw) ? raw.ToUpperInvariant() : null;
    }

    // Warna otomatis (deterministik) berdasarkan nama department, supaya konsisten setiap kali dibaca.
    private static string AutoColorFor(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return AutoColorPalette[0];

        var hash = 0;
        foreach (var ch in name.Trim().ToLowerInvariant())
            hash = (hash * 31 + ch) & 0x7FFFFFFF;

        return AutoColorPalette[hash % AutoColorPalette.Length];
    }

    private DepartmentResponseDto MapDepartment(Department department) => new()
    {
        Id = department.Id,
        Name = department.Name,
        Code = department.Code,
        Description = department.Description,
        IsActive = department.IsActive,
        LogoUrl = string.IsNullOrWhiteSpace(department.LogoUrl)
            ? null
            : _storage.GetPresignedUrl(department.LogoUrl),
        Color = string.IsNullOrWhiteSpace(department.Color)
            ? AutoColorFor(department.Name)
            : department.Color,
        CreatedAt = department.CreatedAt,
        UpdatedAt = department.UpdatedAt
    };

    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();

        return Ok(departments.Select(MapDepartment));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var department = await _db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        return Ok(MapDepartment(department));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var exists = await _db.Departments
            .AnyAsync(d => d.Name == dto.Name || d.Code == dto.Code);

        if (exists)
            return BadRequest(new { message = "Department dengan nama atau kode yang sama sudah ada" });

        var color = NormalizeColor(dto.Color);
        if (!string.IsNullOrWhiteSpace(dto.Color) && color is null)
            return BadRequest(new { message = "Format warna tidak valid. Gunakan format HEX, contoh #2563EB" });

        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Color = color ?? AutoColorFor(dto.Name),
            CreatedAt = DateTime.UtcNow
        };

        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, MapDepartment(department));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _db.Departments.FindAsync(id);

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        var duplicate = await _db.Departments
            .AnyAsync(d => d.Id != id && (d.Name == dto.Name || d.Code == dto.Code));

        if (duplicate)
            return BadRequest(new { message = "Department dengan nama atau kode yang sama sudah ada" });

        var color = NormalizeColor(dto.Color);
        if (!string.IsNullOrWhiteSpace(dto.Color) && color is null)
            return BadRequest(new { message = "Format warna tidak valid. Gunakan format HEX, contoh #2563EB" });

        department.Name = dto.Name;
        department.Code = dto.Code;
        department.Description = dto.Description;
        if (color is not null)
            department.Color = color;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapDepartment(department));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isActive)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        department.IsActive = isActive;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = $"Department berhasil di{(isActive ? "aktifkan" : "nonaktifkan")}" });
    }

    [HttpPost("{id}/logo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        const long maxLogoSize = 5 * 1024 * 1024;
        if (file.Length > maxLogoSize)
            return BadRequest(new { message = "Ukuran file maksimal 5 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "File harus berupa gambar (jpg/png/jpeg)" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Ekstensi file tidak valid" });

        // Validasi magic bytes supaya isi file sesuai dengan ekstensi yang dideklarasikan
        var (isValid, errorMessage) = _fileValidation.Validate(file);
        if (!isValid)
            return BadRequest(new { message = errorMessage });

        // Hapus logo lama supaya tidak menumpuk di storage
        if (!string.IsNullOrWhiteSpace(department.LogoUrl))
            await _storage.DeleteFileAsync(department.LogoUrl);

        var key = await _storage.UploadFileAsync(file);
        if (key is null)
            return BadRequest(new { message = "Gagal menyimpan file" });

        department.LogoUrl = key;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapDepartment(department));
    }

    [HttpDelete("{id}/logo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveLogo(Guid id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        if (string.IsNullOrWhiteSpace(department.LogoUrl))
            return BadRequest(new { message = "Tidak ada logo yang bisa dihapus" });

        await _storage.DeleteFileAsync(department.LogoUrl);

        department.LogoUrl = null;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapDepartment(department));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await _db.Departments.FindAsync(id);

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        if (!string.IsNullOrWhiteSpace(department.LogoUrl))
            await _storage.DeleteFileAsync(department.LogoUrl);

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Department berhasil dihapus" });
    }
}
