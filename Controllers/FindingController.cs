using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModelAttribute]
public class FindingController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetAll(
        [FromQuery] FindingStatus? status,
        [FromQuery] FindingCategory? category,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = db.Findings.Include(f => f.Reporter).AsQueryable();
        if (status.HasValue) query = query.Where(f => f.Status == status);
        if (category.HasValue) query = query.Where(f => f.Category == category);
        if (from.HasValue) query = query.Where(f => f.FoundAt >= from.Value);
        if (to.HasValue) query = query.Where(f => f.FoundAt <= to.Value);
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var finding = await db.Findings.Include(f => f.Reporter).FirstOrDefaultAsync(f => f.Id == id);
        return finding is null ? NotFound() : Ok(finding);
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
            .Include(f => f.Reporter)
            .Where(f => f.SessionId == sessionId)
            .ToListAsync();

        return Ok(new
        {
            total = findings.Count,
            data = findings.Select(f => new
            {
                f.Id, f.Title, f.Department, f.Category,
                f.Description, f.ClauseRef, f.FoundAt, f.Status,
                f.SessionId, f.ChecklistItemId,
                f.ReporterName,
                f.ReporterId,
                ReporterFullName = f.Reporter?.FullName
            })
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> Create([FromBody] CreateFindingRequest req)
    {
        // Auto-resolve ReporterName dari ReporterId kalau diisi
        string reporterName = req.ReporterName;
        if (req.ReporterId.HasValue && string.IsNullOrEmpty(reporterName))
        {
            var reporter = await db.Users.FindAsync(req.ReporterId.Value);
            reporterName = reporter?.FullName ?? string.Empty;
        }

        var finding = new Finding
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Department = req.Department,
            SessionId = req.SessionId,
            ChecklistItemId = req.ChecklistItemId,
            ReporterName = reporterName,
            ReporterId = req.ReporterId,
            Category = req.Category ?? FindingCategory.MinorNC,
            Description = req.Description,
            ClauseRef = req.ClauseRef,
            FoundAt = DateTime.UtcNow,
            Status = FindingStatus.Open
        };
        db.Findings.Add(finding);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = finding.Id }, finding);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFindingRequest req)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound();

        if (req.ReporterId.HasValue)
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
        finding.Category = req.Category ?? finding.Category;
        finding.Description = req.Description;
        finding.ClauseRef = req.ClauseRef;
        await db.SaveChangesAsync();
        return Ok(MapToResponseDto(finding));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] FindingStatus status)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound();
        finding.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var finding = await db.Findings.FindAsync(id);
        if (finding is null) return NotFound();
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
        Category = f.Category,
        Description = f.Description,
        ClauseRef = f.ClauseRef,
        FoundAt = f.FoundAt,
        Status = f.Status
    };
}
