using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Roles = "Admin")]
public class DepartmentController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var department = await _db.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var exists = await _db.Departments
            .AnyAsync(d => d.Name == dto.Name || d.Code == dto.Code);

        if (exists)
            return BadRequest(new { message = "Department dengan nama atau kode yang sama sudah ada" });

        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        var response = new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _db.Departments.FindAsync(id);

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        var duplicate = await _db.Departments
            .AnyAsync(d => d.Id != id && (d.Name == dto.Name || d.Code == dto.Code));

        if (duplicate)
            return BadRequest(new { message = "Department dengan nama atau kode yang sama sudah ada" });

        department.Name = dto.Name;
        department.Code = dto.Code;
        department.Description = dto.Description;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var response = new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isActive)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        department.IsActive = isActive;
        department.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Department berhasil di{(isActive ? "aktifkan" : "nonaktifkan")}" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await _db.Departments.FindAsync(id);

        if (department is null)
            return NotFound(new { message = "Department tidak ditemukan" });

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Department berhasil dihapus" });
    }
}
