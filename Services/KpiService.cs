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

        // Belum selesai & deadline sudah lewat
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
        var today = DateTime.UtcNow;

        // Basis dari AuditSchedules (bukan AuditSessions), biar audit yang
        // belum pernah dibuka sesinya tetap kehitung sebagai "assigned"
        var audits = await db.AuditSchedules
            .Where(s => s.AuditorId == userId)
            .Select(s => new
            {
                s.ScheduledDate,
                Session = db.AuditSessions
                    .Where(x => x.ScheduleId == s.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new { x.Status, x.CompletedAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Audit yang dibatalkan tidak dihitung sebagai tugas
        audits = audits
            .Where(a => a.Session is null || a.Session.Status != AuditSessionStatus.Cancelled)
            .ToList();

        var completed = audits
            .Where(a => a.Session is { Status: AuditSessionStatus.Completed })
            .ToList();

        var completedOnTime = completed.Count(a =>
            a.Session!.CompletedAt.HasValue &&
            a.Session.CompletedAt.Value.Date <= a.ScheduledDate.Date);

        var overdueCompleted = completed.Count - completedOnTime;

        // Belum selesai & deadline sudah lewat
        var stalled = audits.Count(a =>
            a.Session is not { Status: AuditSessionStatus.Completed } &&
            a.ScheduledDate.Date < today.Date);

        return new KpiDto
        {
            TotalAssigned = audits.Count,
            TotalCompleted = completed.Count,
            TotalCompletedOnTime = completedOnTime,
            TotalOverdue = overdueCompleted ,
            TotalStalled = stalled,
            OnTimeRate = completed.Count == 0
                ? 0
                : (double)completedOnTime / completed.Count,
            ComplianceScore = audits.Count == 0
                ? 0
                : Math.Round((double)completed.Count / audits.Count * 100, 1),
            QualityScore = completed.Count == 0
                ? 0
                : Math.Round((double)completedOnTime / completed.Count * 100, 1)
        };
    }
}
