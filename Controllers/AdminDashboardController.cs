using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
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
            .Select(a => new RecentActivityDto
            {
                ActivityType = "CapaAction",
                Description = a.Description + " oleh " + (a.DoneBy != null ? a.DoneBy.FullName : "Unknown"),
                Timestamp = a.DoneAt,
                RelatedId = a.CapaId
            })
            .ToListAsync();

        var verifications = await db.CloseOutVerifications
            .Include(v => v.VerifiedBy)
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
            .Include(f => f.Reporter)
            .Where(f => f.ReporterId != null)
            .Select(f => new RecentActivityDto
            {
                ActivityType = "FindingReported",
                Description = $"Melaporkan finding: {f.Title} oleh " + (f.Reporter != null ? f.Reporter.FullName : "Unknown"),
                Timestamp = f.FoundAt,
                RelatedId = f.Id
            })
            .ToListAsync();

        var recentActivities = capaActions
            .Concat(verifications)
            .Concat(reportedFindings)
            .OrderByDescending(a => a.Timestamp)
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