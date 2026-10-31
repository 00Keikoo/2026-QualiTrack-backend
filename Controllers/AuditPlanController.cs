using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/audit-plans")]
[Authorize]
[ValidateModelAttribute]
public class AuditPlanController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditPlanController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetAll()
    {
        var plansQuery = _db.AuditPlans
            .Include(a => a.Schedules)
                .ThenInclude(s => s.Auditor)
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        Guid? currentUserId = null;
        if (User.IsInRole("AuditorInternal"))
        {
            currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            plansQuery = plansQuery.Where(p => p.Schedules.Any(s => s.AuditorId == currentUserId));
        }

        var plans = await plansQuery.ToListAsync();

        var completedTimes = await _db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed)
            .ToDictionaryAsync(s => s.ScheduleId, s => s.CompletedAt);

        var result = plans.Select(plan =>
        {
            var schedules = currentUserId.HasValue
                ? plan.Schedules.Where(s => s.AuditorId == currentUserId).ToList()
                : plan.Schedules.ToList();

            return new AuditPlanResponseDto
            {
                Id = plan.Id,
                Title = plan.Title,
                Year = plan.Year,
                Standard = plan.Standard,
                CreatedAt = plan.CreatedAt,
                Description = plan.Description,
                Priority = plan.Priority,
                TotalSchedules = schedules.Count,
                Schedules = schedules.Select(s => new ScheduleResponseDto
                {
                    Id = s.Id,
                    ClauseRef = s.ClauseRef,
                    AuditorId = s.AuditorId ?? Guid.Empty,
                    AuditorName = s.Auditor != null
                        ? s.Auditor.FullName
                        : s.AuditorName,  // Fallback ke AuditorName jika join gagal
                    ScheduledDate = s.ScheduledDate,
                    Department = s.Department,

                    IsFinished = completedTimes.ContainsKey(s.Id),
                    CompletedAt = completedTimes.GetValueOrDefault(s.Id)
                }).ToList()
            };
        });

        return Ok(new { message = "Data audit plan berhasil diambil", total = plans.Count, data = result });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var plan = await _db.AuditPlans
            .Include(a => a.Schedules)
                .ThenInclude(s => s.Auditor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (plan is null)
            return NotFound(new { message = $"Audit plan dengan ID {id} tidak ditemukan", id = id });



        var completedTimes = await _db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed)
            .ToDictionaryAsync(s => s.ScheduleId, s => s.CompletedAt);



        var result = new AuditPlanResponseDto
        {
            Id = plan.Id,
            Title = plan.Title,
            Year = plan.Year,
            Standard = plan.Standard,
            CreatedAt = plan.CreatedAt,
            Description = plan.Description,
            Priority = plan.Priority,
            TotalSchedules = plan.Schedules.Count,
            Schedules = plan.Schedules.Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                ClauseRef = s.ClauseRef,
                AuditorId = s.AuditorId ?? Guid.Empty,
                AuditorName = s.Auditor != null
                    ? s.Auditor.FullName
                    : s.AuditorName,  // Fallback ke AuditorName jika join gagal
                ScheduledDate = s.ScheduledDate,
                Department = s.Department,
                IsFinished = completedTimes.ContainsKey(s.Id),
                CompletedAt = completedTimes.GetValueOrDefault(s.Id)
            }).ToList()
        };

        return Ok(new { message = "Data audit plan ditemukan", data = result });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> Create([FromBody] CreateAuditPlanDto planDto)
    {
        var plan = new AuditPlan
        {
            Id = Guid.NewGuid(),
            Title = planDto.Title,
            Year = planDto.Year,
            Standard = planDto.Standard,
            CreatedAt = DateTime.UtcNow,
            Description = planDto.Description,
            Priority = planDto.Priority
        };

        if (planDto.Schedules is not null)
        {
            var schedules = new List<AuditSchedule>();
            foreach (var scheduleDto in planDto.Schedules)
            {
                if (!scheduleDto.ScheduledDate.HasValue)
                    return BadRequest(new { message = $"ScheduledDate untuk Clause {scheduleDto.ClauseRef} wajib dipilih." });

                var auditor = await _db.Users
                    .FirstOrDefaultAsync(u => u.Id == scheduleDto.AuditorId);
                if (auditor is null)
                {
                    return BadRequest(new { message = $"Auditor dengan nama {scheduleDto.AuditorId} tidak ditemukan" });
                }

                schedules.Add(new AuditSchedule
                {
                    Id = Guid.NewGuid(),
                    ClauseRef = scheduleDto.ClauseRef,
                    AuditorId = auditor.Id,
                    AuditorName = auditor.FullName,
                    ScheduledDate = DateTime.SpecifyKind(scheduleDto.ScheduledDate!.Value, DateTimeKind.Utc),
                    Department = scheduleDto.Department,
                    AuditPlan = plan
                });
            }
            plan.Schedules = schedules;
        }


        _db.AuditPlans.Add(plan);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = plan.Id },
            new { message = "Audit plan berhasil dibuat", data = new { plan.Id, plan.Title } });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuditPlanDto updatedPlanDto)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var plan = await _db.AuditPlans
            .Include(a => a.Schedules)
            .FirstOrDefaultAsync(a => a.Id == id);

            if (plan is null)
                return NotFound(new { message = $"Audit plan dengan ID {id} tidak ditemukan", id = id });

            plan.Title = updatedPlanDto.Title;
            plan.Year = updatedPlanDto.Year;
            plan.Standard = updatedPlanDto.Standard;
            plan.Description = updatedPlanDto.Description;
            plan.Priority = updatedPlanDto.Priority;

            
            if (updatedPlanDto.Schedules is not null)
            {
                try
                {
                    await SyncSchedulesAsync(plan, updatedPlanDto.Schedules);
                }
                catch (InvalidOperationException ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message });
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            var result = await BuildResponseDtoAsync(plan);
            return Ok(new { message = "Audit plan berhasil di update", data = result });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Terjadi kesalahan internal saat mengupdate audit plan.", detail = ex.Message});
        }

    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var plan = await _db.AuditPlans
            .Include(a => a.Schedules)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (plan is null)
            return NotFound(new { message = $"Audit plan dengan ID {id} tidak ditemukan", id = id });

        var scheduleIds = plan.Schedules.Select(s => s.Id).ToList();

        if (scheduleIds.Any())
        {
            var sessions = await _db.AuditSessions
                .Where(s => scheduleIds.Contains(s.ScheduleId))
                .ToListAsync();

            var sessionIds = sessions.Select(s => s.Id).ToList();

            if (sessionIds.Any())
            {
                var findings = await _db.Findings
                    .Where(f => f.SessionId.HasValue && sessionIds.Contains(f.SessionId.Value))
                    .ToListAsync();
                _db.Findings.RemoveRange(findings);

                var responses = await _db.AuditResponses
                    .Where(r => sessionIds.Contains(r.SessionId))
                    .ToListAsync();
                _db.AuditResponses.RemoveRange(responses);

                _db.AuditSessions.RemoveRange(sessions);
            }
        }

        _db.AuditSchedules.RemoveRange(plan.Schedules);
        _db.AuditPlans.Remove(plan);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Audit plan berhasil dihapus", id = id });
    }

    private async Task SyncSchedulesAsync(AuditPlan plan, List<CreateScheduleDto> dtos)
    {
        var keepIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToHashSet();

        // 1. HAPUS schedule yang tidak ada di request
        var removed = plan.Schedules.Where(s => !keepIds.Contains(s.Id)).ToList();
        if (removed.Any())
        {
            var removedIds = removed.Select(s => s.Id).ToList();
            var hasSession = await _db.AuditSessions.AnyAsync(s => removedIds.Contains(s.ScheduleId));
            if (hasSession)
                throw new InvalidOperationException("Schedule yang sudah memiliki sesi audit tidak bisa dihapus.");

            _db.AuditSchedules.RemoveRange(removed);
        }

        // 2. UPDATE / TAMBAH schedule
        foreach (var dto in dtos)
        {
            // Validasi ScheduledDate (AP-02)
            if (!dto.ScheduledDate.HasValue)
                throw new InvalidOperationException($"ScheduledDate untuk Clause {dto.ClauseRef} wajib diisi.");

            var auditor = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.AuditorId)
                ?? throw new InvalidOperationException($"Auditor dengan nama {dto.AuditorId} tidak ditemukan");

            var date = DateTime.SpecifyKind(dto.ScheduledDate.Value, DateTimeKind.Utc);
            var existing = dto.Id.HasValue ? plan.Schedules.FirstOrDefault(s => s.Id == dto.Id.Value) : null;

            if (existing is not null)
            {
                var isCompleted = await _db.AuditSessions
                    .AnyAsync(s => s.ScheduleId == existing.Id && s.Status == AuditSessionStatus.Completed);

                if (isCompleted)
                    throw new InvalidOperationException($"Jadwal (Clause: {existing.ClauseRef}) tidak dapat diubah karena audit sudah selesai.");

                existing.ClauseRef = dto.ClauseRef;
                existing.AuditorId = auditor.Id;
                existing.AuditorName = auditor.FullName;
                existing.ScheduledDate = date;
                existing.Department = dto.Department;
            }
            else
            {
                _db.AuditSchedules.Add(new AuditSchedule
                {
                    Id = Guid.NewGuid(),
                    ClauseRef = dto.ClauseRef,
                    AuditorId = auditor.Id,
                    AuditorName = auditor.FullName,
                    ScheduledDate = date,
                    Department = dto.Department,
                    AuditPlanId = plan.Id
                });
            }
        }
    }

    private async Task<AuditPlanResponseDto> BuildResponseDtoAsync(AuditPlan plan)
    {
        var completedTimes = await _db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed)
            .ToDictionaryAsync(s => s.ScheduleId, s => s.CompletedAt);

        return new AuditPlanResponseDto
        {
            Id = plan.Id,
            Title = plan.Title,
            Year = plan.Year,
            Standard = plan.Standard,
            CreatedAt = plan.CreatedAt,
            Description = plan.Description,
            Priority = plan.Priority,
            TotalSchedules = plan.Schedules.Count,
            Schedules = plan.Schedules.Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                ClauseRef = s.ClauseRef,
                AuditorId = s.AuditorId ?? Guid.Empty,
                AuditorName = s.AuditorName,
                ScheduledDate = s.ScheduledDate,
                Department = s.Department,
                IsFinished = completedTimes.ContainsKey(s.Id),
                CompletedAt = completedTimes.GetValueOrDefault(s.Id)
            }).ToList()
        };
    }

    private async Task DeleteRelatedSessionAsync(List<Guid> scheduleIds)
    {
        var sessions = await _db.AuditSessions
            .Where(s => scheduleIds.Contains(s.ScheduleId))
            .ToListAsync();

        var sessionIds = sessions.Select(s => s.Id).ToList();

        if (sessionIds.Any())
        {
            var findings = await _db.Findings
                .Where(f => f.SessionId.HasValue && sessionIds.Contains(f.SessionId.Value))
                .ToListAsync();
            _db.Findings.RemoveRange(findings);

            var responses = await _db.AuditResponses
                .Where(r => sessionIds.Contains(r.SessionId))
                .ToListAsync();
            _db.AuditResponses.RemoveRange(responses);

            _db.AuditSessions.RemoveRange(sessions);
        }
    }
}
