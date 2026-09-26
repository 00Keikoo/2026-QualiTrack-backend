using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QualiTrack.Data;
using QualiTrack.Models;
using QualiTrack.DTOs;
using QualiTrack.Filters;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModelAttribute]
public class CapaController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetAll([FromQuery] CAPAStatus? status)
    {
        var query = db.CAPAs
            .Include(c => c.Actions)
                .ThenInclude(a => a.DoneBy)
            .Include(c => c.CloseOut)
                .ThenInclude(co => co!.VerifiedById)
            .Include(c => c.Pic)
            .Include(c => c.Finding)
            .AsQueryable();

        if (status.HasValue) query = query.Where(c => c.Status == status);

        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            query = query.Where(c => c.PicId == userId);
        }
        var capas = await query.ToListAsync();
        var response = capas.Select(MapToResponseDto).ToList();

        return Ok(response);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetOverdue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = db.CAPAs
            .Where(c => c.Deadline < today && c.Status != CAPAStatus.Closed)
            .Include(c => c.Actions)
            .Include(c => c.Pic)
            .Include(c => c.Finding)
            .AsQueryable();

        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            query = query.Where(c => c.PicId == userId);
        }

        var overdue  = await query.ToListAsync();
        var response = overdue.Select(c => MapToResponseDto(c)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var capa = await db.CAPAs
            .Include(c => c.Actions)
            .Include(c => c.CloseOut)
            .Include(c => c.Pic)
            .Include(c => c.Finding)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (capa is null) return NotFound();

        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (capa.PicId != userId) return Forbid();
        }
        return Ok(MapToResponseDto(capa));
    }

    [HttpPost("finding/{findingId}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> Create(Guid findingId, [FromBody] CreateCapaRequest req)
    {
        var finding = await db.Findings.FindAsync(findingId);
        if (finding is null) return NotFound("Finding tidak ditemukan");

        if (!req.Deadline.HasValue)
            return BadRequest(new { message = "Deadline wajib diisi" });

        if (!req.PicId.HasValue || req.PicId == Guid.Empty)
        {
            if (string.IsNullOrEmpty(req.PicName))
                return BadRequest(new { message = "PIC harus diisi dengan PicId atau PicName" });

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.FullName == req.PicName);
            if (user is null)
                return BadRequest(new { message = "PIC tidak ditemukan berdasarkan nama" });

            req.PicId = user.Id;
        }

        var capa = new CAPA
        {
            Id = Guid.NewGuid(),
            FindingId = findingId,
            RootCause = req.RootCause,
            CorrectiveAction = req.CorrectiveAction,
            PreventiveAction = req.PreventiveAction,
            PicId = req.PicId.Value,
            Deadline = req.Deadline.Value,
            Status = CAPAStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        db.CAPAs.Add(capa);
        finding.Status = FindingStatus.InProgress;
        await db.SaveChangesAsync();

        var createdCapa = await db.CAPAs
            .Include(c => c.Actions)
            .Include(c => c.Pic)
            .FirstOrDefaultAsync(c => c.Id == capa.Id);

        if (createdCapa is null)
            return StatusCode(500, new { message = "Gagal memuat ulang CAPA setelah dibuat" });
        return CreatedAtAction(nameof(GetById), new { id = capa.Id }, MapToResponseDto(createdCapa));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCapaRequestDto req)
    {
        var capa = await db.CAPAs.FindAsync(id);
        if (capa is null) return NotFound();

        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (capa.PicId != userId) return Forbid();
        }

        if (!req.Deadline.HasValue)
            return BadRequest(new { message = "Deadline wajib diisi" });
        if (req.PicId.HasValue && req.PicId == Guid.Empty)
            return BadRequest(new { message = "PIC tidak boleh Guid kosong" });

        capa.RootCause = req.RootCause;
        capa.CorrectiveAction = req.CorrectiveAction;
        capa.PreventiveAction = req.PreventiveAction;
        capa.Deadline = req.Deadline.Value;
        if (req.PicId.HasValue)
            capa.PicId = req.PicId.Value;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] CAPAStatus status)
    {
        var capa = await db.CAPAs.Include(c => c.CloseOut).FirstOrDefaultAsync(c => c.Id == id);
        if (capa is null) return NotFound();
        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (capa.PicId != userId) return Forbid();
        }

        if (capa.Status == CAPAStatus.Closed && capa.CloseOut != null)
            return BadRequest(new { message = "CAPA yang sudah ditutup dan terverifikasi tidak dapat diubah statusnya secara manual" });
            
        capa.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/actions")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> AddAction(Guid id, [FromBody] AddCapaActionRequest req)
    {
        var capa = await db.CAPAs.FindAsync(id);
        if (capa is null) return NotFound();
        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (capa.PicId != userId) return Forbid();
        }
        if (!req.DoneById.HasValue || req.DoneById == Guid.Empty)
            return BadRequest(new { message = "DoneById harus diisi dan bukan Guid kosong" });

        var action = new CAPAAction
        {
            Id = Guid.NewGuid(),
            CapaId = id,
            Description = req.Description,
            DoneById = req.DoneById.Value,
            DoneAt = DateTime.UtcNow
        };

        db.CAPAActions.Add(action);
        if (capa.Status != CAPAStatus.Closed)
            capa.Status = CAPAStatus.InProgress;
        await db.SaveChangesAsync();

        var doneBy = await db.Users.FindAsync(req.DoneById.Value);
        var response = new CAPAActionResponseDto
        {
            Id = action.Id,
            CapaId = action.CapaId,
            Description = action.Description,
            DoneById = action.DoneById,
            DoneByName = doneBy?.FullName,
            DoneAt = action.DoneAt
        };
        return Ok(response);
    }

    [HttpPost("{id}/closeout")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> CloseOut(Guid id, [FromBody] CloseOutVerificationRequest req)
    {
        var capa = await db.CAPAs.FirstOrDefaultAsync(c => c.Id == id);

        if (capa is null) return NotFound();
        if (User.IsInRole("Auditee"))
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (capa.PicId != userId) return Forbid();
        }
        if (!req.IsEffective.HasValue)
            return BadRequest(new { message = "IsEffective wajib diisi" });
        if (!req.VerifiedById.HasValue || req.VerifiedById == Guid.Empty)
            return BadRequest(new { message = "VerifiedById harus diisi dan bukan Guid kosong" });

        var verification = new CloseOutVerification
        {
            Id = Guid.NewGuid(),
            CapaId = id,
            IsEffective = req.IsEffective.Value,
            VerificationNotes = req.VerificationNotes,
            VerifiedById = req.VerifiedById.Value,
            VerifiedAt = DateTime.UtcNow
        };

        db.CloseOutVerifications.Add(verification);
        capa.Status = CAPAStatus.Closed;
        capa.ClosedAt = DateTime.UtcNow;

        await db.Findings
            .Where(f => f.Id == capa.FindingId)
            .ExecuteUpdateAsync(f => f.SetProperty(f => f.Status, FindingStatus.Closed));

        await db.SaveChangesAsync();

        var verifiedBy = await db.Users.FindAsync(req.VerifiedById.Value);
        var response = new CloseOutResponseDto
        {
            Id = verification.Id,
            CapaId = verification.CapaId,
            IsEffective = verification.IsEffective,
            VerificationNotes = verification.VerificationNotes,
            VerifiedById = verification.VerifiedById,
            VerifiedByName = verifiedBy?.FullName,
            VerifiedAt = verification.VerifiedAt
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var capa = await db.CAPAs.FindAsync(id);
        if (capa is null) return NotFound();
        db.CAPAs.Remove(capa);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private CAPAResponseDto MapToResponseDto(CAPA capa)
    {
        return new CAPAResponseDto
        {
            Id = capa.Id,
            FindingId = capa.FindingId,
            FindingTitle = capa.Finding != null
                ? (string.IsNullOrEmpty(capa.Finding.ClauseRef) ? capa.Finding.Title : $"{capa.Finding.ClauseRef} - {capa.Finding.Title}")
                    : string.Empty,
            FindingCategory = capa.Finding != null ? capa.Finding.Category.ToString() : string.Empty,

            RootCause = capa.RootCause,
            CorrectiveAction = capa.CorrectiveAction,
            PreventiveAction = capa.PreventiveAction,
            Deadline = capa.Deadline,
            Status = capa.Status,
            PicId = capa.PicId,
            PicName = capa.Pic?.FullName,
            CreatedAt = capa.CreatedAt,
            ClosedAt = capa.ClosedAt,
            Actions = capa.Actions.Select(a => new CAPAActionResponseDto
            {
                Id = a.Id,
                CapaId = a.CapaId,
                Description = a.Description,
                DoneById = a.DoneById,
                DoneByName = a.DoneBy?.FullName,
                DoneAt = a.DoneAt
            }).ToList(),
            CloseOut = capa.CloseOut == null ? null : new CloseOutResponseDto
            {
                Id = capa.CloseOut.Id,
                CapaId = capa.CloseOut.CapaId,
                IsEffective = capa.CloseOut.IsEffective,
                VerificationNotes = capa.CloseOut.VerificationNotes,
                VerifiedById = capa.CloseOut.VerifiedById,
                VerifiedByName = capa.CloseOut.VerifiedBy?.FullName,
                VerifiedAt = capa.CloseOut.VerifiedAt
            }
        };
    }
}
