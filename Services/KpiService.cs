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

        var kpi = new KpiDto
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

        var role = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync();

        var today = DateTime.UtcNow.Date;

        if (role == UserRoles.Auditee)
        {
            // Auditee: tugasnya adalah CAPA yang ditugaskan ke dia (PIC)
            var openPastDeadline = userCapas.Count(c =>
                c.Status != CAPAStatus.Closed &&
                c.Deadline < DateOnly.FromDateTime(today));

            SetTaskKpi(kpi, "Capa",
                assigned: userCapas.Count,
                completed: closedCapas.Count,
                completedOnTime: closedOnTime,
                notCompletedPastDue: openPastDeadline);
            return kpi;
        }

        // Auditor: tugasnya adalah jadwal audit yang ditugaskan ke dia + sesi terakhirnya
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

        var completedAudits = audits
            .Where(a => a.Session is { Status: AuditSessionStatus.Completed })
            .ToList();

        var auditsOnTime = completedAudits.Count(a =>
            a.Session!.CompletedAt.HasValue &&
            a.Session.CompletedAt.Value.Date <= a.ScheduledDate.Date);

        var auditsPastDue = audits.Count(a =>
            a.Session is not { Status: AuditSessionStatus.Completed } &&
            a.ScheduledDate.Date < today);

        SetTaskKpi(kpi, "Audit",
            assigned: audits.Count,
            completed: completedAudits.Count,
            completedOnTime: auditsOnTime,
            notCompletedPastDue: auditsPastDue);
        return kpi;
    }

    // Overdue = selesai lewat tenggat + belum selesai padahal tenggat sudah lewat
    private static void SetTaskKpi(KpiDto kpi, string basis, int assigned, int completed, int completedOnTime, int notCompletedPastDue)
    {
        kpi.KpiBasis = basis;
        kpi.TotalAssigned = assigned;
        kpi.TotalCompleted = completed;
        kpi.TotalCompletedOnTime = completedOnTime;
        kpi.TotalOverdue = (completed - completedOnTime) + notCompletedPastDue;
        kpi.QualityScore = completed == 0 ? 0 : (double)completedOnTime / completed;
        kpi.SuccessRate = assigned == 0 ? 0 : (double)completed / assigned;
    }
}
