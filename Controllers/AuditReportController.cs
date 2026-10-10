using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/audit-reports")]
[Authorize]
public class AuditReportController(AppDbContext db) : ControllerBase
{
    [HttpGet]

    public async Task<IActionResult> GetAll(
        [FromQuery] string? standard,
        [FromQuery] string? department,
        [FromQuery] int? year)
    {
        var sessions = await db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed)
            .Include(s => s.Schedule)
                .ThenInclude(sch => sch.AuditPlan)
            .Include(s => s.Schedule)
                .ThenInclude(sch => sch.Auditor)
            .Include(s => s.Summary)
            .Include(s => s.Findings)
                .ThenInclude(f => f.Category!)
            .Include(s => s.Findings)
                .ThenInclude(f => f.Capa)
                    .ThenInclude(c => c.Status!)
            .ToListAsync();

        if (!string.IsNullOrEmpty(standard))
            sessions = sessions.Where(s => s.Schedule.AuditPlan.Standard == standard).ToList();
        if (!string.IsNullOrEmpty(department))
            sessions = sessions.Where(s => s.Schedule.Department == department).ToList();
        if (year.HasValue)
            sessions = sessions.Where(s => s.Schedule.AuditPlan.Year == year).ToList();

        var result = sessions.Select(s => new AuditReportListItemDto
        {
            AuditPlanId = s.Schedule.AuditPlanId,
            AuditPlanTitle = s.Schedule.AuditPlan.Title,
            Standard = s.Schedule.AuditPlan.Standard,
            Priority = s.Schedule.AuditPlan.Priority,
            ScheduleId = s.ScheduleId,
            Department = s.Schedule.Department,
            AuditorName = s.Schedule.Auditor?.FullName ?? s.Schedule.AuditorName,
            ScheduledDate = s.Schedule.ScheduledDate,
            SessionId = s.Id,
            CompletedAt = s.CompletedAt,
            SummaryContent = s.Summary?.Content,
            TotalFindings = s.Findings.Count,
            TotalMajorNC = s.Findings.Count(f => f.Category.Name == "MajorNC"),
            TotalMinorNC = s.Findings.Count(f => f.Category.Name == "MinorNC"),
            TotalCAPAs = s.Findings.Count(f => f.Capa != null),
            ClosedCAPAs = s.Findings.Count(f => f.Capa != null && f.Capa.Status.IsTerminal)
        }).OrderByDescending(r => r.CompletedAt).ToList();

        return Ok(new { message = "Data laporan audit berhasil diambil", total = result.Count, data = result });
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetDetail(Guid sessionId)
    {
        var session = await db.AuditSessions
            .Where(s => s.Id == sessionId)
            .Include(s => s.Schedule).ThenInclude(sch => sch.AuditPlan)
            .Include(s => s.Schedule).ThenInclude(sch => sch.Auditor)
            .Include(s => s.Summary)
            .Include(s => s.Findings).ThenInclude(f => f.Capa).ThenInclude(c => c!.Pic)
            .FirstOrDefaultAsync();

        if (session is null)
            return NotFound(new { message = "Session tidak ditemukan" });

        var result = new AuditReportDetailDto
        {
            AuditPlanId = session.Schedule.AuditPlanId,
            AuditPlanTitle = session.Schedule.AuditPlan.Title,
            Standard = session.Schedule.AuditPlan.Standard,
            Description = session.Schedule.AuditPlan.Description,
            Priority = session.Schedule.AuditPlan.Priority,
            Year = session.Schedule.AuditPlan.Year,
            ScheduleId = session.ScheduleId,
            Department = session.Schedule.Department,
            AuditorName = session.Schedule.Auditor?.FullName ?? session.Schedule.AuditorName,
            ScheduledDate = session.Schedule.ScheduledDate,
            SessionId = session.Id,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            SummaryContent = session.Summary?.Content,
            Findings = session.Findings.Select(f => new FindingReportDto
            {
                Id = f.Id,
                Title = f.Title,
                Category = f.Category.Name,
                Description = f.Description,
                ClauseRef = f.ClauseRef,
                Status = f.Status!.ToString(),
                FoundAt = f.FoundAt,
                Capa = f.Capa == null ? null : new CapaReportDto
                {
                    Id = f.Capa.Id,
                    RootCause = f.Capa.RootCause,
                    CorrectiveAction = f.Capa.CorrectiveAction,
                    PreventiveAction = f.Capa.PreventiveAction,
                    PicName = f.Capa.Pic?.FullName ?? string.Empty,
                    Deadline = f.Capa.Deadline.ToString("yyyy-MM-dd"),
                    Status = f.Capa.Status.Name
                }
            }).ToList()
        };

        return Ok(new { message = "Detail laporan audit berhasil diambil", data = result });
    }
}
