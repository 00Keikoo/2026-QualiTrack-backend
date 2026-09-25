using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class KpiService(AppDbContext db) : IKpiService
{
    public async Task<KpiDto> GetUserKpiAsync(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return new KpiDto();

        return user.Role switch
        {
            "AuditorInternal" => await GetAuditorKpiAsync(userId),
            "Auditee" => await GetAuditeeKpiAsync(userId),
            _ => new KpiDto()
        };
    }

    private async Task<KpiDto> GetAuditeeKpiAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var userCapas = await db.CAPAs
            .Where(c => c.PicId == userId)
            .ToListAsync();

        var closedCapas = userCapas.Where(c => c.Status == CAPAStatus.Closed).ToList();

        var closedOnTime = closedCapas.Count(c =>
            c.ClosedAt.HasValue &&
            DateOnly.FromDateTime(c.ClosedAt.Value) <= c.Deadline);

        var overdue = closedCapas.Count(c =>
            c.ClosedAt.HasValue &&
            DateOnly.FromDateTime(c.ClosedAt.Value) > c.Deadline);

        var stalled = userCapas.Count(c =>
            c.Status != CAPAStatus.Closed &&
            c.Deadline < today);

        var totalFindingReported = await db.Findings
            .CountAsync(f => f.ReporterId == userId);

        return new KpiDto
        {
            TotalAssigned = userCapas.Count,
            TotalCompleted = closedCapas.Count,
            TotalCompletedOnTime = closedOnTime,
            TotalOverdue = overdue,
            TotalStalled = stalled,
            OnTimeRate = closedCapas.Count == 0
                ? 0
                : (double)closedOnTime / closedCapas.Count,
            TotalFindingsReported = totalFindingReported,
            ComplianceScore = userCapas.Count == 0
                ? 0
                : Math.Round((double)closedCapas.Count / userCapas.Count * 100, 1),
            QualityScore = closedCapas.Count == 0
                ? 0
                : Math.Round((double)closedOnTime / closedCapas.Count * 100, 1)
        };
    }

    private async Task<KpiDto> GetAuditorKpiAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        var sessions = await db.AuditSessions
            .Include(s => s.Schedule)
            .Where(s => s.Schedule.AuditorId == userId)
            .ToListAsync();

        var completed = sessions.Where(s => s.Status == AuditSessionStatus.Completed).ToList();

        var completedOnTime = completed.Count(s =>
            s.CompletedAt.HasValue &&
            s.CompletedAt.Value <= s.Schedule.ScheduledDate);

        // Selesai TAPI telat dari deadline
        var overdue = completed.Count(s =>
            s.CompletedAt.HasValue &&
            s.CompletedAt.Value > s.Schedule.ScheduledDate);

        // BELUM selesai & deadline udah lewat sekarang
        var stalled = sessions.Count(s =>
            s.Status != AuditSessionStatus.Completed &&
            s.Schedule.ScheduledDate < now);

        return new KpiDto
        {
            TotalAssigned = sessions.Count,
            TotalCompleted = completed.Count,
            TotalCompletedOnTime = completedOnTime,
            TotalOverdue = overdue,
            TotalStalled = stalled,
            OnTimeRate = completed.Count == 0
                ? 0
                : (double)completedOnTime / completed.Count,
            // Compliance: sudah dikerjain atau belum (telat tetap dihitung selesai)
            ComplianceScore = sessions.Count == 0
                ? 0
                : Math.Round((double)completed.Count / sessions.Count * 100, 1),
            // Quality: dari yang selesai, seberapa tepat waktu
            QualityScore = completed.Count == 0
                ? 0
                : Math.Round((double)completedOnTime / completed.Count * 100, 1)
        };
    }
}
