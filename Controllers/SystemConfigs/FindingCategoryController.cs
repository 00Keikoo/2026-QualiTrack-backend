using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs.SystemConfigs;
using QualiTrack.Models;

namespace QualiTrack.Controllers.SystemConfig;

[ApiController]
[Route("api/system-config/finding-categories")]
[Authorize(Roles = "Admin")]
public class FindingCategoryController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetFindingCategories()
    {
        var categories = await _db.FindingCategories
            .AsNoTracking()
            .OrderBy(c => c.Severity)
            .Select(c => new FindingCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Severity = c.Severity,
                RequiresImmediateAction = c.RequiresImmediateAction,
                IsActive = c.IsActive,
                UsageCount = c.Findings.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFindingCategory([FromBody] CreateFindingCategoryDto dto)
    {
        var exists = await _db.FindingCategories
            .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            return BadRequest(new { message = "Kategori finding dengan nama yang sama sudah ada" });

        var category = new FindingCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Severity = dto.Severity,
            RequiresImmediateAction = dto.RequiresImmediateAction,
            CreatedAt = DateTime.UtcNow
        };

        _db.FindingCategories.Add(category);
        await _db.SaveChangesAsync();

        var response = new FindingCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Severity = category.Severity,
            RequiresImmediateAction = category.RequiresImmediateAction,
            IsActive = category.IsActive,
            UsageCount = 0,
            CreatedAt = category.CreatedAt
        };

        return CreatedAtAction(nameof(GetFindingCategories), new { id = category.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFindingCategory(Guid id, [FromBody] UpdateFindingCategoryDto dto)
    {
        var category = await _db.FindingCategories.FindAsync(id);

        if (category is null)
            return NotFound(new { message = "Kategori finding tidak ditemukan" });

        var duplicate = await _db.FindingCategories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower());

        if (duplicate)
            return BadRequest(new { message = "Kategori finding dengan nama yang sama sudah ada" });

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Severity = dto.Severity;
        category.RequiresImmediateAction = dto.RequiresImmediateAction;

        await _db.SaveChangesAsync();

        var response = new FindingCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Severity = category.Severity,
            RequiresImmediateAction = category.RequiresImmediateAction,
            IsActive = category.IsActive,
            UsageCount = await _db.Findings.CountAsync(f => f.CategoryId == id),
            CreatedAt = category.CreatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFindingCategory(Guid id)
    {
        var category = await _db.FindingCategories
            .Include(c => c.Findings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound(new { message = "Kategori finding tidak ditemukan" });

        if (category.Findings.Any())
            return Conflict(new
            {
                message = "Kategori finding tidak dapat dihapus karena masih digunakan oleh finding",
                usageCount = category.Findings.Count
            });

        _db.FindingCategories.Remove(category);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Kategori finding berhasil dihapus" });
    }
}