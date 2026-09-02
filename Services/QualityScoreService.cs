using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class QualityScoreService(AppDbContext db) : IQualityScoreService
{
    public async Task<List<QualityTrendDto>> GetQualityTrendsAsync(int year)
    {
        var sessions = await db.AuditSessions
            .Include(s => s.Schedule)
            .Include(s => s.Responses)
            .Where(s => s.Status == AuditSessionStatus.Completed
                && s.CompletedAt.HasValue
                && s.CompletedAt.Value.Year == year)
            .ToListAsync();

        var grouped = sessions
            .GroupBy(s => s.CompletedAt!.Value.Month)
            .Select(g =>
                {
                    int totalItems = 0;
                    int totalConform = 0;
                    int totalAdjustedConform = 0;

                    foreach (var session in g)
                    {
                        var conformCount = session.Responses.Count(r => r.Answer == ResponseAnswer.Conform);
                        var itemCount = session.Responses.Count;
                        var isOverdue = session.CompletedAt.HasValue && session.CompletedAt.Value > session.Schedule.ScheduledDate;
                        var adjustedConform = isOverdue ? Math.Max(0, conformCount - 1) : conformCount;

                        totalItems += itemCount;
                        totalConform += conformCount;
                        totalAdjustedConform += adjustedConform;
                    }
                    return new QualityTrendDto
                    {
                        Period = g.Key,
                        PeriodLabel = new DateTime(year, g.Key, 1).ToString("MMM yyyy"),
                        TotalSessions = g.Count(),
                        ComplianceScore = totalItems == 0 ? 0 : Math.Round((double)totalConform / totalItems * 100, 1),
                        QualityScore = totalConform == 0 ? 0 : Math.Round((double)totalAdjustedConform / totalConform * 100, 1)
                    };
                })
            .OrderBy(x => x.Period)
            .ToList();
        return grouped;
    }

    public async Task<List<DepartmentTrendDto>> GetDepartmentComplianceTrendAsync(string timeframe)
    {
        DateTime? cutoff = timeframe.ToLower() switch
        {
            "3m" => DateTime.UtcNow.AddMonths(-3),
            "6m" => DateTime.UtcNow.AddMonths(-6),
            "1y" => DateTime.UtcNow.AddMonths(-12),
            "all" => null,
            _ => throw new ArgumentException("Timeframe harus salah satu dari : 3m, 6m, 1y, all")
        };
        var query = db.AuditSessions
            .Include(s => s.Schedule)
            .Include(s => s.Responses)
            .Where(s => s.Status == AuditSessionStatus.Completed && s.CompletedAt.HasValue)
            .AsQueryable();

        if (cutoff.HasValue)
            query = query.Where(s => s.CompletedAt!.Value >= cutoff.Value);

        var sessions = await query.ToListAsync();

        var result = sessions
            .GroupBy(s => s.Schedule.Department)
            .Select(deptGroup => new DepartmentTrendDto
            {
                Department = deptGroup.Key,
                Data = deptGroup
                    .GroupBy(s => new { s.CompletedAt!.Value.Year, s.CompletedAt.Value.Month })
                    .Select(periodGroup =>
                        {
                            var totalItems = periodGroup.Sum(s => s.Responses.Count);
                            var totalConform = periodGroup.Sum(s => s.Responses.Count(r => r.Answer == ResponseAnswer.Conform));

                            return new PeriodScoreDto
                            {
                                Year = periodGroup.Key.Year,
                                Month = periodGroup.Key.Month,
                                PeriodLabel = new DateTime(periodGroup.Key.Year, periodGroup.Key.Month, 1).ToString("MMM yyyy"),
                                TotalSessions = periodGroup.Count(),
                                ComplianceScore = totalItems == 0 ? 0 : Math.Round((double)totalConform / totalItems * 100, 1)
                            };
                        })
                        .OrderBy(p => p.Year).ThenBy(p => p.Month)
                        .ToList()
            })
            .OrderBy(d => d.Department)
            .ToList();

        return result;
    }
}
