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
public class AuditResponseController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditResponseController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/AuditResponse/batch
    [HttpPost("batch")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> SaveBatch([FromBody] BatchAuditResponseDto dto)
    {
        var session = await _db.AuditSessions.FindAsync(dto.SessionId);
        if (session is null)
            return NotFound(new { message = $"Session {dto.SessionId} tidak ditemukan" });

        if (session.Status == AuditSessionStatus.Completed)
            return BadRequest(new { message = "Session sudah selesai, jawaban tidak bisa diubah" });

        // Idempotent — hapus jawaban lama lalu simpan baru
        var existing = await _db.AuditResponses
            .Where(r => r.SessionId == dto.SessionId)
            .ToListAsync();
        if (existing.Any())
            _db.AuditResponses.RemoveRange(existing);

        var responses = dto.Responses.Select(r => new AuditResponse
        {
            Id = Guid.NewGuid(),
            SessionId = dto.SessionId,
            ChecklistItemId = r.ChecklistItemId,
            Answer = r.IsPassed ? ResponseAnswer.Conform : ResponseAnswer.NotConform,
            Notes = r.Notes
        }).ToList();

        _db.AuditResponses.AddRange(responses);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Jawaban berhasil disimpan",
            total = responses.Count
        });
    }

    // POST /api/AuditResponse/progress
// Simpan satu jawaban — dipanggil setiap user jawab satu item
    [HttpPost("progress")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressDto dto)
    {
        var session = await _db.AuditSessions.FindAsync(dto.SessionId);
        if (session is null)
            return NotFound(new { message = $"Session {dto.SessionId} tidak ditemukan" });

        if (session.Status == AuditSessionStatus.Completed)
            return BadRequest(new { message = "Session sudah selesai" });

        var checkListItem = await _db.ChecklistItems.FindAsync(dto.ChecklistItemId);
        if (checkListItem is null)
            return NotFound(new { message = $"Checklist item {dto.ChecklistItemId} tidak ditemukan" });
        
        // Cek apakah sudah ada jawaban untuk item ini
        var existing = await _db.AuditResponses
            .FirstOrDefaultAsync(r => r.SessionId == dto.SessionId 
                                && r.ChecklistItemId == dto.ChecklistItemId);

        if (existing is not null)
        {
            // Update jawaban yang sudah ada
            existing.Answer = dto.IsPassed ? ResponseAnswer.Conform : ResponseAnswer.NotConform;
            existing.Notes = dto.Notes;
        }
        else
        {
            // Buat jawaban baru
            _db.AuditResponses.Add(new AuditResponse
            {
                Id = Guid.NewGuid(),
                SessionId = dto.SessionId,
                ChecklistItemId = dto.ChecklistItemId,
                Answer = dto.IsPassed ? ResponseAnswer.Conform : ResponseAnswer.NotConform,
                Notes = dto.Notes
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Progress tersimpan" });
    }

    // GET /api/AuditResponse/by-session/{sessionId}
    [HttpGet("by-session/{sessionId:guid}")]
    [Authorize(Roles = "Admin, QualityManager, AuditorInternal, Auditee")]
    public async Task<IActionResult> GetBySession(Guid sessionId)
    {
        var responses = await _db.AuditResponses
            .Include(r => r.ChecklistItem)
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();

        return Ok(new
        {
            total = responses.Count,
            data = responses.Select(r => new
            {
                id = r.Id,
                checklistItemId = r.ChecklistItemId,
                question = r.ChecklistItem?.Question,
                isPassed = r.Answer == ResponseAnswer.Conform,
                answer = r.Answer.ToString(),
                notes = r.Notes
            })
        });
    }
}