using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(AppDbContext db) : ControllerBase
{
    // ============================================================
    // GET /api/Dashboard/summary
    // Audit Summary: active audit, total capa, capa open, capa overdue
    // ============================================================
    [HttpGet("summary")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Filter audit session berdasarkan tahun (misal dari createdAt / schedule)
        var activeAudit = await db.AuditSessions
            .CountAsync(s => s.Status == AuditSessionStatus.InProgress);

        var totalCapa = await db.CAPAs
            .CountAsync();

        var capaOpen = await db.CAPAs
            .CountAsync(c => c.Status == CAPAStatus.Open || c.Status == CAPAStatus.InProgress);

        var capaOverdue = await db.CAPAs
            .CountAsync(c => c.Deadline < today && c.Status != CAPAStatus.Closed);

        return Ok(new
        {
            activeAudit,
            totalCapa,
            capaOpen,
            capaOverdue
        });
    }

    // ============================================================
    // GET /api/Dashboard/compliance-score
    // Compliance Score per department
    // ============================================================
    [HttpGet("compliance-score")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetComplianceScore([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        // Ambil semua session yang completed beserta responses
        var sessions = await db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed
                     && s.Schedule != null
                     && s.Schedule.ScheduledDate.Year == targetYear)
            .Include(s => s.Schedule)
            .Include(s => s.Responses)
            .ToListAsync();

        if (!sessions.Any())
            return Ok(new { message = "Belum ada audit yang selesai", data = new List<object>() });

        // Hitung compliance score per department
        var scorePerDepartment = sessions
            .Where(s => s.Schedule != null && s.Responses.Any())
            .GroupBy(s => s.Schedule.Department)
            .Select(g =>
            {
                var totalResponses = g.Sum(s => s.Responses.Count);
                var conformResponses = g.Sum(s => s.Responses
                    .Count(r => r.Answer == ResponseAnswer.Conform));

                var score = totalResponses > 0
                    ? Math.Round((double)conformResponses / totalResponses * 100, 1)
                    : 0;

                var monthlyBreakdown = g
                    .GroupBy(s => s.Schedule.ScheduledDate.Month)
                    .Select(mg =>
                    {
                        var mTotal = mg.Sum(s => s.Responses.Count);
                        var mConform = mg.Sum(s => s.Responses
                            .Count(r => r.Answer == ResponseAnswer.Conform));
                        return new
                        {
                            month = mg.Key,
                            score = mTotal > 0
                                ? Math.Round((double)mConform / mTotal * 100, 1)
                                : 0,
                            totalResponses = mTotal,
                            conformResponses = mConform
                        };
                    })
                    .OrderBy(x => x.month)
                    .ToList();
                return new
                {
                    department = g.Key,
                    score,
                    totalAudit = g.Count(),
                    totalResponses,
                    conformResponses
                };
            })
            .OrderByDescending(x => x.score)
            .ToList();

        // Overall compliance score
        var totalAll = scorePerDepartment.Sum(x => x.totalResponses);
        var conformAll = scorePerDepartment.Sum(x => x.conformResponses);
        var overallScore = totalAll > 0
            ? Math.Round((double)conformAll / totalAll * 100, 1)
            : 0;

        return Ok(new
        {
            overallScore,
            data = scorePerDepartment
        });
    }

    // GET /api/Dashboard/audit-schedule?month=10&year=2023
    [HttpGet("audit-schedule")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetAuditSchedule(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var schedules = await db.AuditSchedules
            .Include(s => s.AuditPlan)
            .Where(s => s.ScheduledDate.Month == targetMonth
                    && s.ScheduledDate.Year == targetYear)
            .OrderBy(s => s.ScheduledDate)
            .ToListAsync();

        // Group by tanggal, tiap tanggal berisi list department
        var result = schedules
            .GroupBy(s => s.ScheduledDate.Day)
            .Select(g => new
            {
                day = g.Key,
                departments = g.Select(s => new
                {
                    department = s.Department,
                    scheduleId = s.Id,
                    planTitle = s.AuditPlan?.Title,
                    standard = s.AuditPlan?.Standard
                }).ToList()
            })
            .OrderBy(x => x.day)
            .ToList();

        return Ok(new
        {
            month = targetMonth,
            year = targetYear,
            data = result
        });
    }

    // ============================================================
    // GET /api/Dashboard/monthly-report?month=5&year=2026
    // Monthly Compliance Report untuk PDF
    // ============================================================
    [HttpGet("monthly-report")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        // Audit schedules bulan ini
        var schedules = await db.AuditSchedules
            .Include(s => s.AuditPlan)
            .Include(s => s.Auditor)
            .Where(s => s.ScheduledDate.Month == targetMonth
                     && s.ScheduledDate.Year == targetYear)
            .ToListAsync();

        var scheduleIds = schedules.Select(s => s.Id).ToList();

        // Sessions bulan ini
        var sessions = await db.AuditSessions
            .Include(s => s.Responses)
            .Where(s => scheduleIds.Contains(s.ScheduleId))
            .ToListAsync();

        // Findings bulan ini
        var sessionIds = sessions.Select(s => s.Id).ToList();
        var findings = await db.Findings
            .Where(f => f.SessionId.HasValue && sessionIds.Contains(f.SessionId.Value))
            .ToListAsync();

        // CAPA bulan ini
        var findingIds = findings.Select(f => f.Id).ToList();
        var capas = await db.CAPAs
            .Where(c => findingIds.Contains(c.FindingId))
            .ToListAsync();

        // Hitung compliance score
        var totalResponses = sessions.Sum(s => s.Responses.Count);
        var conformResponses = sessions.Sum(s => s.Responses
            .Count(r => r.Answer == ResponseAnswer.Conform));
        var complianceScore = totalResponses > 0
            ? Math.Round((double)conformResponses / totalResponses * 100, 1)
            : 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return Ok(new
        {
            month = targetMonth,
            year = targetYear,
            generatedAt = DateTime.UtcNow,
            summary = new
            {
                totalSchedules = schedules.Count,
                completedAudit = sessions.Count(s => s.Status == AuditSessionStatus.Completed),
                inProgressAudit = sessions.Count(s => s.Status == AuditSessionStatus.InProgress),
                notStartedAudit = schedules.Count - sessions.Count,
                complianceScore,
                totalFindings = findings.Count,
                totalCapa = capas.Count,
                capaOpen = capas.Count(c => c.Status == CAPAStatus.Open),
                capaOverdue = capas.Count(c => c.Deadline < today && c.Status != CAPAStatus.Closed)
            },
            schedules = schedules.Select(s =>
            {
                var session = sessions.FirstOrDefault(ses => ses.ScheduleId == s.Id);
                var sessionFindings = session != null
                    ? findings.Where(f => f.SessionId == session.Id).ToList()
                    : new List<Finding>();

                return new
                {
                    department = s.Department,
                    auditorName = s.Auditor?.FullName ?? s.AuditorName,
                    scheduledDate = s.ScheduledDate,
                    status = session == null ? "NotStarted" : session.Status.ToString(),
                    sessionId = session?.Id,
                    totalFindings = sessionFindings.Count,
                    majorNC = sessionFindings.Count(f => f.Category == FindingCategory.MajorNC),
                    minorNC = sessionFindings.Count(f => f.Category == FindingCategory.MinorNC),
                    observation = sessionFindings.Count(f => f.Category == FindingCategory.Observation)
                };
            })
        });
    }
}