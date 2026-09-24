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
public class AuditSessionController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditSessionController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/AuditSession
    [HttpPost]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> Create([FromBody] CreateAuditSessionDto dto)
    {
        var schedule = await _db.AuditSchedules.FindAsync(dto.ScheduleId);
        if (schedule is null)
            return NotFound(new { message = $"Schedule {dto.ScheduleId} tidak ditemukan" });

        var checklist = await _db.Checklists.FindAsync(dto.ChecklistId);
        if (checklist is null)
            return NotFound(new { message = $"Checklist {dto.ChecklistId} tidak ditemukan" });

        // Cegah duplikat session untuk schedule yang sama
        var existing = await _db.AuditSessions
            .FirstOrDefaultAsync(s => s.ScheduleId == dto.ScheduleId );

        if (existing is not null)
            return Ok(new
            {
                message = "Sesi audit sudah ada",
                data = new {sessionId = existing.Id, status = existing.Status.ToString()}
            });

        var session = new AuditSession
        {
            Id = Guid.NewGuid(),
            ScheduleId = dto.ScheduleId,
            ChecklistId = dto.ChecklistId,
            StartedAt = DateTime.UtcNow,
            Status = AuditSessionStatus.InProgress,
            Notes = dto.Notes
        };

        _db.AuditSessions.Add(session);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = session.Id },
            new { message = "Audit session dimulai", data = ToDto(session) });
    }

    // GET /api/AuditSession/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, QualityManager, AuditorInternal, Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var session = await _db.AuditSessions.FindAsync(id);
        if (session is null)
            return NotFound(new { message = $"Session {id} tidak ditemukan" });

        return Ok(new { data = ToDto(session) });
    }

    // GET /api/AuditSession/by-schedule/{scheduleId}
    [HttpGet("by-schedule/{scheduleId}")]
    [Authorize(Roles = "Admin, QualityManager, AuditorInternal, Auditee")]
    public async Task<IActionResult> GetBySchedule(Guid scheduleId)
    {
        var session = await _db.AuditSessions
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (session is null)
            return NotFound(new { message = "Session belum dibuat untuk schedule ini" });

        return Ok(new { data = ToDto(session) });
    }

    // PATCH /api/AuditSession/{id}/complete
    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var session = await _db.AuditSessions.FindAsync(id);
        if (session is null)
            return NotFound(new { message = $"Session {id} tidak ditemukan" });

        if (session.Status == AuditSessionStatus.Completed)
            return BadRequest(new { message = "Session sudah selesai" });

        session.Status = AuditSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Audit session selesai", data = ToDto(session) });
    }

    // PATCH /api/AuditSession/{id}/cancel
    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var session = await _db.AuditSessions.FindAsync(id);
        if (session is null)
            return NotFound(new { message = $"Session {id} tidak ditemukan" });

        if (session.Status == AuditSessionStatus.Completed)
            return BadRequest(new { message = "Session sudah selesai, tidak bisa dibatalkan" });

        session.Status = AuditSessionStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Audit session dibatalkan", data = ToDto(session) });
    }

    // POST /api/AuditSession/{sessionId}/summary
    [HttpPost("{sessionId}/summary")]
    [Authorize(Roles = "Admin, QualityManager, AuditorInternal, Auditee")]
    public async Task<IActionResult> CreateSummary(Guid sessionId, [FromBody] CreateAuditSummaryDto dto)
    {
        var session = await _db.AuditSessions
            .Include(s => s.Summary)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null)
            return NotFound(new { message = $"Session {sessionId} not found" });

        if (session.Status == AuditSessionStatus.Cancelled)
            return BadRequest(new { message = "Cancelled session cannot be summarized" });

        if (session.Summary is not null)
            return BadRequest(new { message = "Summary already exists for this session" });

        var summary = new AuditSummary
        {
            Id = Guid.NewGuid(),
            AuditSessionId = sessionId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        // Sekalian complete session
        session.Status = AuditSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;

        _db.AuditSummaries.Add(summary);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSummary), new { sessionId },
            new { message = "Summary saved and audit session completed", data = ToSummaryDto(summary) });
    }

    // GET /api/AuditSession/{sessionId}/summary
    [HttpGet("{sessionId}/summary")]
    [Authorize(Roles = "Admin, QualityManager, AuditorInternal, Auditee")]
    public async Task<IActionResult> GetSummary(Guid sessionId)
    {
        var summary = await _db.AuditSummaries
            .FirstOrDefaultAsync(s => s.AuditSessionId == sessionId);

        if (summary is null)
            return NotFound(new { message = "Summary not found for this session" });

        return Ok(new { data = ToSummaryDto(summary) });
    }

    private static AuditSummaryResponseDto ToSummaryDto(AuditSummary s) => new()
    {
        Id = s.Id,
        AuditSessionId = s.AuditSessionId,
        Content = s.Content,
        CreatedAt = s.CreatedAt
    };

    private static AuditSessionResponseDto ToDto(AuditSession s) => new()
    {
        Id = s.Id,
        ScheduleId = s.ScheduleId,
        ChecklistId = s.ChecklistId,
        Status = s.Status.ToString(),
        StartedAt = s.StartedAt,
        CompletedAt = s.CompletedAt,
        Notes = s.Notes
    };
}
