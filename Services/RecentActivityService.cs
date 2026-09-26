using Microsoft.EntityFrameworkCore;
using QualiTrack.DTOs;
using QualiTrack.Data;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class RecentActivityService(AppDbContext db) : IRecentActivityService
{
    public async Task<List<RecentActivityDto>> GetUserRecentActivityAsync(Guid userId, int limit = 10)
    {
        var capaActions = await db.CAPAActions
            .Where(a => a.DoneById == userId)
            .Select(a => new RecentActivityDto
            {
                ActivityType = "CapaAction",
                Description = a.Description,
                Timestamp = a.DoneAt,
                RelatedId = a.CapaId
            })
            .ToListAsync();

        var verifications = await db.CloseOutVerifications
            .Where(v => v.VerifiedById == userId)
            .Select(v => new RecentActivityDto
            {
                ActivityType = "CapaVerified",
                Description = v.IsEffective
                    ? "Memverifikasi CAPA sebagai efektif"
                    : "Memverifikasi CAPA sebagai tidak efektif",
                Timestamp = v.VerifiedAt,
                RelatedId = v.CapaId
            })
            .ToListAsync();

        var reportedFindings = await db.Findings
            .Where(f => f.ReporterId == userId)
            .Select(f => new RecentActivityDto
            {
                ActivityType = "FindingReported",
                Description = $"Melaporkan finding: {f.Title}",
                Timestamp = f.FoundAt,
                RelatedId = f.Id
            })
            .ToListAsync();

        var completedAudits = await db.AuditSessions
            .Include(s => s.Schedule)
            .Where(s => s.Schedule.AuditorId == userId
                && s.Status == AuditSessionStatus.Completed
                && s.CompletedAt.HasValue)
            .Select(s => new RecentActivityDto
            {
                ActivityType = "AuditCompleted",
                Description = $"Menyelesaikan audit: {s.Schedule.ClauseRef} - {s.Schedule.Department}",
                Timestamp = s.CompletedAt!.Value,
                RelatedId = s.Id
            })
            .ToListAsync();

        return capaActions
            .Concat(verifications)
            .Concat(reportedFindings)
            .Concat(completedAudits)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToList();
    }
}