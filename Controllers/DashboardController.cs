using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QualiTrack.Constants;
using QualiTrack.Data;
using QualiTrack.Models;
using QualiTrack.Helpers;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(AppDbContext db, IMemoryCache cache) : ControllerBase
{
    [HttpGet("summary")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var cacheKey = $"dashboard_summary_{targetYear}";
        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var today = DateHelper.TodayWib();

            var activeAudit = await db.AuditSessions
                .CountAsync(s => s.Status == AuditSessionStatus.InProgress
                    && (!year.HasValue || s.Schedule.AuditPlan.Year == year.Value));

            var totalCapa = await db.CAPAs.CountAsync();

            var capaOpen = await db.CAPAs
                .CountAsync(c => c.Status != null && !c.Status.IsTerminal);

            var capaOverdue = await db.CAPAs
                .CountAsync(c => c.Deadline < today && c.Status != null && !c.Status.IsTerminal);

            return new
            {
                activeAudit,
                totalCapa,
                capaOpen,
                capaOverdue
            };
        });

        return Ok(result);
    }

    [HttpGet("compliance-score")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetComplianceScore([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var sessions = await db.AuditSessions
            .Where(s => s.Status == AuditSessionStatus.Completed
                     && s.Schedule != null
                     && s.Schedule.ScheduledDate.Year == targetYear)
            .Include(s => s.Schedule)
            .Include(s => s.Responses)
            .ToListAsync();

        if (!sessions.Any())
            return Ok(new { message = "Belum ada audit yang selesai", data = new List<object>() });

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
                        conformResponses,
                        monthlyBreakdown
                    };
                })
                .OrderByDescending(x => x.score)
                .ToList();

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

    [HttpGet("audit-schedule")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
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

    [HttpGet("monthly-report")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal,Auditee")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var schedulesQuery = db.AuditSchedules
            .Where(s => s.ScheduledDate.Month == targetMonth
                && s.ScheduledDate.Year == targetYear);

        var schedules = await schedulesQuery
            .Include(s => s.AuditPlan)
            .Include(s => s.Auditor)
            .ToListAsync();

        var scheduleIds = await schedulesQuery.Select(s => s.Id).ToListAsync();

        var sessions = await db.AuditSessions
            .Where(s => scheduleIds.Contains(s.ScheduleId))
            .Include(s => s.Responses)
            .ToListAsync();

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var findings = await db.Findings
            .Include(f => f.Category)
            .Where(f => f.SessionId.HasValue && sessionIds.Contains(f.SessionId.Value))
            .Include(f => f.Category)
            .ToListAsync();

        var findingIds = findings.Select(f => f.Id).ToList();
        var capas = await db.CAPAs
            .Where(c => findingIds.Contains(c.FindingId))
            .Include(c => c.Status)
            .ToListAsync();

        var totalResponses = sessions.Sum(s => s.Responses.Count);
        var conformResponses = sessions.Sum(s => s.Responses
            .Count(r => r.Answer == ResponseAnswer.Conform));
        var complianceScore = totalResponses > 0
            ? Math.Round((double)conformResponses / totalResponses * 100, 1)
            : 0;

        var today = DateHelper.TodayWib();

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
                notStartedAudit = Math.Max(0, schedules.Count - sessions.Count),
                complianceScore,
                totalFindings = findings.Count,
                totalCapa = capas.Count,
                capaOpen = capas.Count(c => !c.Status!.IsTerminal),
                capaOverdue = capas.Count(c => c.Deadline < today && !c.Status!.IsTerminal)
            },
            schedules = schedules.Select(s =>
            {
                var session = sessions.FirstOrDefault(ses => ses.ScheduleId == s.Id);
                var sessionFindings = session != null
                    ? findings.Where(f => f.SessionId == session.Id).ToList()
                    : new List<Finding>();

                return new
                {
                    scheduleId = s.Id,
                    planId = s.AuditPlanId,
                    department = s.Department,
                    auditorName = s.Auditor?.FullName ?? s.AuditorName,
                    scheduledDate = s.ScheduledDate,
                    status = session == null ? "NotStarted" : session.Status.ToString(),
                    sessionId = session?.Id,
                    totalFindings = sessionFindings.Count,
                    majorNC = sessionFindings.Count(f => f.Category!.Name == DefaultCategories.Major),
                    minorNC = sessionFindings.Count(f => f.Category!.Name == DefaultCategories.Minor),
                    observation = sessionFindings.Count(f => f.Category!.Name == DefaultCategories.Observation)
                };
            })
        });
    }
}