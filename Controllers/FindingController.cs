using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using QualiTrack.Models;
using QualiTrack.Constants;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/findings")]
[Authorize]
[ValidateModelAttribute]
public class FindingController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetAll(
        [FromQuery] FindingStatus? status,
        [FromQuery] string? categoryName,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = db.Findings
            .Include(f => f.Category)
            .Include(f => f.Reporter)
            .AsQueryable();
        if (status.HasValue) query = query.Where(f => f.Status == status);
        if (!string.IsNullOrEmpty(categoryName))
            query = query.Where(f => f.Category!.Name == categoryName);
        if (from.HasValue) query = query.Where(f => f.FoundAt >= from.Value);
        if (to.HasValue) query = query.Where(f => f.FoundAt <= to.Value);

        var findings = await query.ToListAsync();
        var result = findings.Select(MapToResponseDto).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var finding = await db.Findings
            .Include(f => f.Reporter)
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (finding is null) return NotFound();

        return Ok(MapToResponseDto(finding));
    }

    [HttpGet("without-capa")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetWithoutCapa()
    {
        var findingsWithCapa = await db.CAPAs.Select(c => c.FindingId).ToListAsync();
        var findings = await db.Findings
            .Include(f => f.Reporter)
            .Where(f => !findingsWithCapa.Contains(f.Id))
            .ToListAsync();
        return Ok(new { total = findings.Count, data = findings });
    }

    [HttpGet("by-session/{sessionId:guid}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetBySession(Guid sessionId)
    {
        var findings = await db.Findings
            .Include(f => f.Category)
            .Include(f => f.Reporter)
            .Where(f => f.SessionId == sessionId)
            .ToListAsync();

        var result = findings.Select(MapToResponseDto).ToList();

        return Ok(new
        {
            total = result.Count,
            data = result
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> Create([FromBody] CreateFindingRequest req)
    {
        // Auto-resolve ReporterName dari ReporterId kalau diisi
        string reporterName = req.ReporterName;
        Guid? reporterId = req.ReporterId;

        // Auditee hanya boleh membuat finding atas nama dirinya sendiri, jadi
        // ReporterId/Nama dipaksa berasal dari klaim JWT (tidak bisa dipalsukan).
        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var reporter = await db.Users.FindAsync(userId);
            reporterId = userId;
            reporterName = reporter?.FullName ?? reporterName;
        }
        else if (reporterId.HasValue && string.IsNullOrEmpty(reporterName))
        {
            var reporter = await db.Users.FindAsync(reporterId.Value);
            reporterName = reporter?.FullName ?? string.Empty;
        }

        Guid categoryId;
        if(req.CategoryId.HasValue && req.CategoryId != Guid.Empty)
        {
            var category = await db.FindingCategories
                .FirstOrDefaultAsync(c => c.Id == req.CategoryId.Value && c.IsActive);
            if(category == null)
                return BadRequest(new {message = "Category tidak valid atau tidak aktif"});
            categoryId = category.Id;
        }
        else
        {
            // Jika frontend tidak mengirim, otomatis gunakan Minor NC
            var minorCategory = await db.FindingCategories
                .FirstOrDefaultAsync(c => c.Name == DefaultCategories.Minor && c.IsActive);
            if (minorCategory == null)
                return BadRequest(new { message = $"Category default '{DefaultCategories.Minor}' tidak ditemukan di database" });
        
            categoryId = minorCategory.Id;
        }

        var finding = new Finding
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Department = req.Department,
            SessionId = req.SessionId,
            ChecklistItemId = req.ChecklistItemId,
            ReporterName = reporterName,
            ReporterId = reporterId,
            CategoryId = categoryId,
            Description = req.Description,
            ClauseRef = req.ClauseRef,
            FoundAt = DateTime.UtcNow,
            Status = FindingStatus.Open
        };
        db.Findings.Add(finding);
        await db.SaveChangesAsync();
        var created = await db.Findings
            .Include(f => f.Category)
            .Include(f => f.Reporter)
            .FirstOrDefaultAsync(f => f.Id == finding.Id);

        return CreatedAtAction(nameof(GetById), new { id = finding.Id }, MapToResponseDto(created!));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFindingRequest req)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound(new { message = "Finding tidak ditemukan" });

        // Auditee hanya boleh mengedit finding yang dia buat sendiri.
        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (finding.ReporterId != userId)
                return Forbid();
        }
        else if (req.ReporterId.HasValue)
        {
            finding.ReporterId = req.ReporterId;
            if (!string.IsNullOrEmpty(req.ReporterName))
            {
                finding.ReporterName = req.ReporterName;
            }
            else
            {
                var reporter = await db.Users.FindAsync(req.ReporterId.Value);
                finding.ReporterName = reporter?.FullName ?? finding.ReporterName;
            }
        }

        finding.Title = req.Title;
        finding.Department = req.Department;
        finding.Description = req.Description;
        finding.ClauseRef = req.ClauseRef;

        if (req.CategoryId.HasValue)
        {
            var category = await db.FindingCategories
                .FirstOrDefaultAsync(c => c.Id == req.CategoryId && c.IsActive);
            if (category == null)
                return BadRequest(new { message = "Category tidak valid atau tidak aktif" });
            finding.CategoryId = req.CategoryId;
        }
        await db.SaveChangesAsync();
        return Ok(MapToResponseDto(finding));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] FindingStatus status)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound(new { message = "Finding tidak ditemukan" });
        finding.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Hanya Admin yang boleh menghapus finding (Auditee dan role lain tidak bisa delete).
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound(new { message = "Finding tidak ditemukan" });
        db.Findings.Remove(finding);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static FindingResponseDto MapToResponseDto(Finding f) => new()
    {
        Id = f.Id,
        SessionId = f.SessionId,
        ChecklistItemId = f.ChecklistItemId,
        ReporterName = f.ReporterName,
        ReporterId = f.ReporterId,
        Title = f.Title,
        Department = f.Department,
        CategoryId = f.CategoryId,
        CategoryName = f.Category?.Name,
        Description = f.Description,
        ClauseRef = f.ClauseRef,
        FoundAt = f.FoundAt,
        Status = f.Status
    };
}
