using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Models;
using System.Security.Claims;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController(AppDbContext db) : ControllerBase
{
    // GET /api/admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Total counts
        var totalUsers = await db.Users.CountAsync();
        var totalIsoStandards = await db.IsoStandards.CountAsync();
        var totalDepartments = await db.Departments.CountAsync();
        var totalChecklistTemplates = await db.Checklists.CountAsync();
        var totalSpcUnits = await db.SpcUnits.CountAsync();

        // All user recent activities — dari semua user
        var capaActions = await db.CAPAActions
            .Include(a => a.DoneBy)
            .Select(a => new
            {
                activityType = "CapaAction",
                description = a.Description,
                userName = a.DoneBy != null ? a.DoneBy.FullName : "Unknown",
                timestamp = a.DoneAt
            })
            .ToListAsync();

        var verifications = await db.CloseOutVerifications
            .Include(v => v.VerifiedBy)
            .Select(v => new
            {
                activityType = "CapaVerified",
                description = v.IsEffective
                    ? "Memverifikasi CAPA sebagai efektif"
                    : "Memverifikasi CAPA sebagai tidak efektif",
                userName = v.VerifiedBy != null ? v.VerifiedBy.FullName : "Unknown",
                timestamp = v.VerifiedAt
            })
            .ToListAsync();
        
        var reportedFindings = await db.Findings
            .Include(f => f.Reporter)
            .Where(f => f.ReporterId != null)
            .Select(f => new
            {
                activityType = "FindingReported",
                description = $"Melaporkan finding: {f.Title}",
                userName = f.Reporter != null ? f.Reporter.FullName : "Unknown",
                timestamp = f.FoundAt
            })
            .ToListAsync();

        var recentActivities = capaActions
            .Cast<dynamic>()
            .Concat(verifications.Cast<dynamic>())
            .Concat(reportedFindings.Cast<dynamic>())
            .OrderByDescending(a => (DateTime)a.timestamp)
            .Take(10)
            .ToList();


        // Recent changes — aktivitas Admin
        var recentChanges = await db.AdminActivityLogs
            .Where(l => l.AdminId == adminId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => new
            {
                action = l.Action,
                entityType = l.EntityType,
                entityName = l.EntityName,
                description = l.Description,
                timestamp = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            totalUsers,
            totalIsoStandards,
            totalDepartments,
            totalChecklistTemplates,
            totalSpcUnits,
            recentActivities,
            recentChanges
        });
    }
}